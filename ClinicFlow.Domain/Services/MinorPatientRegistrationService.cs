using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Patients;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.Services.Registration;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Registers a dependent minor patient by reception: validates the minor boundary,
/// refuses suspected duplicates without an override reason, creates the profile, and
/// delegates the guardian membership rules to the family link service.
/// </summary>
public static class MinorPatientRegistrationService
{
    public static (Patient Patient, FamilyMembership Membership) Register(
        MinorPatientRegistrationArgs args,
        MinorPatientRegistrationContext context
    )
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.FirstName);
        ArgumentNullException.ThrowIfNull(args.FirstSurname);
        ArgumentNullException.ThrowIfNull(args.HomeAddress);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.DuplicateCandidates);
        ArgumentNullException.ThrowIfNull(context.GuardianPatient);
        ArgumentNullException.ThrowIfNull(context.GuardianSelfMembership);

        if (context.ReceptionistUserId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.InvalidValue);

        var fullName = PersonName.Create(args.FirstName, args.MiddleName, args.FirstSurname);
        var patientAge = DomainRules.ComputeAge(
            args.DateOfBirth,
            DateOnly.FromDateTime(context.ReferenceTime)
        );

        if (patientAge >= DomainRules.AdultAge)
            throw new DomainValidationException(DomainErrors.PatientRegistration.MinorRequired);

        var scoring = MinorPatientDuplicateScoringService.Score(
            new MinorPatientDuplicateScoringArgs
            {
                FirstName = args.FirstName,
                FirstSurname = args.FirstSurname,
                DateOfBirth = args.DateOfBirth,
                SexAtBirth = args.SexAtBirth,
                HomeAddress = args.HomeAddress,
                HasSsnLast4 = args.HasSsnLast4,
            },
            new PatientDuplicateScoringContext { Candidates = context.DuplicateCandidates }
        );

        scoring.EnsureRegistrationAllowed(args.DuplicateOverrideReason);

        var patient = Patient.CreateProfile(
            fullName,
            args.DateOfBirth,
            args.SexAtBirth,
            args.HomeAddress,
            context.ReferenceTime,
            args.DuplicateOverrideReason
        );

        var membership = MinorFamilyLinkService.Link(
            patient,
            new MinorFamilyLinkArgs
            {
                Relationship = args.Relationship,
                LegalAuthority = args.LegalAuthority,
            },
            new MinorFamilyLinkContext
            {
                GuardianPatient = context.GuardianPatient,
                GuardianSelfMembership = context.GuardianSelfMembership,
                ActiveFamilyMemberCount = context.ActiveFamilyMemberCount,
                ReferenceTime = context.ReferenceTime,
            }
        );

        patient.AddDomainEvent(
            new PatientRegisteredByReceptionEvent(
                patient.Id,
                context.ReceptionistUserId,
                context.ReferenceTime
            )
        );

        return (patient, membership);
    }
}

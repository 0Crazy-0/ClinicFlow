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
/// Registers an adult or independent minor patient by reception with a self membership:
/// validates the minimum self membership age of 12 years, refuses suspected duplicates
/// without an override reason, creates the profile, and wires the self membership to
/// the pending account the handler provides.
/// </summary>
public static class PatientRegistrationService
{
    public const int MinimumSelfMembershipAge = 12;

    public static (Patient Patient, User PendingUser, FamilyMembership SelfMembership) Register(
        PatientRegistrationArgs args,
        PatientRegistrationContext context
    )
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.FirstName);
        ArgumentNullException.ThrowIfNull(args.FirstSurname);
        ArgumentNullException.ThrowIfNull(args.HomeAddress);
        ArgumentNullException.ThrowIfNull(args.Email);
        ArgumentNullException.ThrowIfNull(args.Phone);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.DuplicateCandidates);
        ArgumentNullException.ThrowIfNull(context.PendingUser);

        if (context.ReceptionistUserId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.InvalidValue);

        var fullName = PersonName.Create(args.FirstName, args.MiddleName, args.FirstSurname);
        var patientAge = DomainRules.ComputeAge(
            args.DateOfBirth,
            DateOnly.FromDateTime(context.ReferenceTime)
        );

        if (patientAge < MinimumSelfMembershipAge)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.PatientTooYoungForSelfMembership
            );

        var scoring = PatientDuplicateScoringService.Score(
            new PatientDuplicateScoringArgs
            {
                FirstName = args.FirstName,
                FirstSurname = args.FirstSurname,
                DateOfBirth = args.DateOfBirth,
                SexAtBirth = args.SexAtBirth,
                HomeAddress = args.HomeAddress,
                HasSsnLast4 = args.HasSsnLast4,
                Email = args.Email,
                Phone = args.Phone,
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

        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            context.PendingUser.Id,
            context.ReferenceTime
        );

        patient.AddDomainEvent(
            new PatientRegisteredByReceptionEvent(
                patient.Id,
                context.ReceptionistUserId,
                context.ReferenceTime
            )
        );

        return (patient, context.PendingUser, membership);
    }
}

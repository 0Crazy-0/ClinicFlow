using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Registers a primary patient profile, either by reusing an existing clinical
/// record or creating a new one, and links it to the account via a new
/// self-referencing FamilyMembership.
/// </summary>
public static class PrimaryProfileRegistrationService
{
    public const int MinimumSelfAge = 12;

    public static (Patient Patient, FamilyMembership Membership) Register(
        PrimaryProfileRegistrationArgs args,
        PrimaryProfileRegistrationContext context
    )
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.FullName);
        ArgumentNullException.ThrowIfNull(context);

        if (context.HasExistingSelfMembership)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.PatientAlreadyHasActiveMembership
            );

        if (
            context.ExistingPatient is not null
            && context.ExistingPatient.SexAtBirth != args.SexAtBirth
        )
            throw new DomainValidationException(DomainErrors.Patient.DataMismatch);

        var patient =
            context.ExistingPatient
            ?? Patient.CreateProfile(
                args.FullName,
                args.DateOfBirth,
                args.SexAtBirth,
                context.ReferenceTime
            );

        if (patient.GetAge(DateOnly.FromDateTime(context.ReferenceTime)) < MinimumSelfAge)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.PatientTooYoungForSelfMembership
            );

        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            context.UserId,
            context.ReferenceTime
        );

        return (patient, membership);
    }
}

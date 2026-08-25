using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Registers a primary patient profile, either by reusing an existing clinical
/// record or creating a new one, and links it to the account via a new
/// self-referencing FamilyMembership.
/// </summary>
/// <remarks>
/// Encapsulates two independent business rules: soft-deleted patients cannot be
/// reused directly and must go through the administrative claim process, and a
/// patient cannot be linked as Self while another Self FamilyMembership for
/// that patient is still active.
/// </remarks>
public static class PrimaryProfileRegistrationService
{
    public const int MinimumSelfAge = 13;

    public static (Patient Patient, FamilyMembership Membership) Register(
        PrimaryProfileRegistrationArgs args
    )
    {
        if (args.HasExistingSelfMembership)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.PatientAlreadyHasActiveMembership
            );

        var patient =
            args.ExistingPatient
            ?? Patient.CreateProfile(args.FullName, args.DateOfBirth, args.ReferenceTime);

        if (patient.GetAge(DateOnly.FromDateTime(args.ReferenceTime)) < MinimumSelfAge)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.PatientTooYoungForSelfMembership
            );

        var membership = FamilyMembership.CreateSelf(patient.Id, args.UserId, args.ReferenceTime);

        return (patient, membership);
    }
}

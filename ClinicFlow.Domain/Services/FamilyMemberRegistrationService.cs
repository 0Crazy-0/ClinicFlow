using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Registers a family member by creating a new profile or reactivating a soft-deleted one.
/// </summary>
/// <remarks>
/// Encapsulates the business rule that prevents duplicate active profiles while
/// allowing soft-deleted profiles to be safely restored.
/// </remarks>
public static class FamilyMemberRegistrationService
{
    public const int MaxActiveFamilyMembers = 15;

    public static Patient Register(
        Patient? existingProfile,
        int activeFamilyMemberCount,
        FamilyMemberRegistrationArgs args
    )
    {
        if (existingProfile is not null && existingProfile.UserId != args.UserId)
            throw new DomainValidationException(DomainErrors.Patient.UserIdMismatch);

        if (existingProfile is not null && !existingProfile.IsDeleted)
            throw new DomainValidationException(DomainErrors.Patient.ActiveProfileAlreadyExists);

        if (activeFamilyMemberCount >= MaxActiveFamilyMembers)
            throw new DomainValidationException(DomainErrors.Patient.FamilyMemberLimitExceeded);

        if (existingProfile is null)
        {
            return Patient.CreateFamilyMember(
                args.UserId,
                args.FullName,
                args.Relationship,
                args.DateOfBirth,
                args.ReferenceTime
            );
        }

        existingProfile.ReactivateAsFamilyMember(args.Relationship);
        return existingProfile;
    }
}

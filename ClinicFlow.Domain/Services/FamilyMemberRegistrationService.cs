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
    public static Patient Register(Patient? existingProfile, FamilyMemberRegistrationArgs args)
    {
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

        if (existingProfile.UserId != args.UserId)
            throw new DomainValidationException(DomainErrors.Patient.UserIdMismatch);

        if (!existingProfile.IsDeleted)
            throw new DomainValidationException(DomainErrors.Patient.ActiveProfileAlreadyExists);

        existingProfile.ReactivateAsFamilyMember(args.Relationship);
        return existingProfile;
    }
}

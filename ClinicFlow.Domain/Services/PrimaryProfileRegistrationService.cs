using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Registers a primary patient profile by creating a new one or reactivating a soft-deleted one.
/// </summary>
/// <remarks>
/// Encapsulates the business rule that prevents duplicate active profiles while
/// allowing soft-deleted profiles to be safely restored.
/// </remarks>
public static class PrimaryProfileRegistrationService
{
    public static Patient Register(Patient? existingProfile, PrimaryProfileRegistrationArgs args)
    {
        if (existingProfile is null)
        {
            return Patient.CreateSelf(
                args.UserId,
                args.FullName,
                args.DateOfBirth,
                args.ReferenceTime
            );
        }

        if (existingProfile.UserId != args.UserId)
            throw new DomainValidationException(DomainErrors.Patient.UserIdMismatch);

        if (!existingProfile.IsDeleted)
            throw new DomainValidationException(DomainErrors.Patient.ActiveProfileAlreadyExists);

        existingProfile.ReactivateAsPrimary();
        return existingProfile;
    }
}

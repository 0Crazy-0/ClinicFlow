using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;

namespace ClinicFlow.Domain.Services;

public static class DoctorRegistrationService
{
    public static Doctor Register(DoctorRegistrationArgs args, DoctorRegistrationContext context)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.FullName);
        ArgumentNullException.ThrowIfNull(args.LicenseNumber);
        ArgumentNullException.ThrowIfNull(args.Room);
        ArgumentNullException.ThrowIfNull(context);

        if (context.ExistingDoctor is not null)
            throw new DomainValidationException(DomainErrors.Doctor.InactiveProfileExists);

        return Doctor.Create(
            context.UserId,
            args.FullName,
            args.LicenseNumber,
            args.MedicalSpecialtyId,
            args.Biography,
            args.Room
        );
    }
}

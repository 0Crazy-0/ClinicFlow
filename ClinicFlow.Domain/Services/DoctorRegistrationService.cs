using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts;

namespace ClinicFlow.Domain.Services;

public static class DoctorRegistrationService
{
    public static Doctor Register(DoctorRegistrationArgs args, DoctorRegistrationContext context)
    {
        if (context.ExistingDoctor is not null)
            throw new DomainValidationException(DomainErrors.Doctor.InactiveProfileExists);

        return Doctor.Create(
            args.UserId,
            args.LicenseNumber,
            args.MedicalSpecialtyId,
            args.Biography,
            args.ConsultationRoom
        );
    }
}

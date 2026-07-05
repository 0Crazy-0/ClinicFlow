using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;

namespace ClinicFlow.Domain.Services;

public static class DoctorRegistrationService
{
    public static Doctor Register(DoctorRegistrationArgs args, Doctor? existingDoctor)
    {
        if (existingDoctor is not null)
            throw new DomainValidationException(DomainErrors.Doctor.InactiveProfileExists);

        return Doctor.Create(
            args.UserId,
            args.FullName,
            args.LicenseNumber,
            args.MedicalSpecialtyId,
            args.Biography,
            args.ConsultationRoom
        );
    }
}

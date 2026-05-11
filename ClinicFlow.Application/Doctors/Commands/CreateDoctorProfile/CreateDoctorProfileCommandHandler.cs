using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;

public sealed class CreateDoctorProfileCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateDoctorProfileCommand, Guid>
{
    public async Task<Guid> Handle(CreateDoctorProfileCommand request, CancellationToken ct)
    {
        var existingDoctor = await doctorRepository.GetIncludingDeletedByLicenseNumberAsync(
            request.LicenseNumber,
            ct
        );

        var context = new DoctorRegistrationContext { ExistingDoctor = existingDoctor };
        var args = new DoctorRegistrationArgs
        {
            UserId = request.UserId,
            LicenseNumber = MedicalLicenseNumber.Create(request.LicenseNumber),
            MedicalSpecialtyId = request.MedicalSpecialtyId,
            Biography = request.Biography,
            ConsultationRoom = ConsultationRoom.Create(
                request.ConsultationRoomNumber,
                request.ConsultationRoomName,
                request.ConsultationRoomFloor
            ),
        };
        var doctor = DoctorRegistrationService.Register(args, context);

        await doctorRepository.CreateAsync(doctor, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return doctor.Id;
    }
}

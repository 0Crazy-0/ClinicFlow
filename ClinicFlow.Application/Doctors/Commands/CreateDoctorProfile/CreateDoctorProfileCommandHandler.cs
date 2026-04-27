using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;

public sealed class CreateDoctorProfileCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateDoctorProfileCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateDoctorProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var consultationRoom = ConsultationRoom.Create(
            request.ConsultationRoomNumber,
            request.ConsultationRoomName,
            request.ConsultationRoomFloor
        );

        var doctor = Doctor.Create(
            request.UserId,
            MedicalLicenseNumber.Create(request.LicenseNumber),
            request.MedicalSpecialtyId,
            request.Biography,
            consultationRoom
        );

        await doctorRepository.CreateAsync(doctor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return doctor.Id;
    }
}

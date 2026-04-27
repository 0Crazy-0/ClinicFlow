using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.UpdateDoctorProfile;

public sealed class UpdateDoctorProfileCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateDoctorProfileCommand>
{
    public async Task Handle(
        UpdateDoctorProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var doctor =
            await doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.DoctorId
            );

        var consultationRoom = ConsultationRoom.Create(
            request.ConsultationRoomNumber,
            request.ConsultationRoomName,
            request.ConsultationRoomFloor
        );

        doctor.UpdateProfile(request.Biography, consultationRoom);

        await doctorRepository.UpdateAsync(doctor, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

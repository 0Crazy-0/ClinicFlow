using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Doctors.Commands.ReactivateDoctorProfile;

public sealed class ReactivateDoctorProfileCommandHandler(
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReactivateDoctorProfileCommand>
{
    public async Task Handle(ReactivateDoctorProfileCommand request, CancellationToken ct)
    {
        var doctor =
            await doctorRepository.GetByIdAsync(request.DoctorId, ct)
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

        doctor.Reactivate(request.Biography, consultationRoom);

        await unitOfWork.SaveChangesAsync(ct);
    }
}

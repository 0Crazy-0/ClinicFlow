using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByDoctor;

public sealed class MarkAppointmentAsNoShowByDoctorCommandHandler(
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<MarkAppointmentAsNoShowByDoctorCommand>
{
    public async Task Handle(MarkAppointmentAsNoShowByDoctorCommand request, CancellationToken ct)
    {
        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        var doctor =
            await doctorRepository.GetByUserIdAsync(request.InitiatorUserId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.InitiatorUserId
            );

        appointment.MarkAsNoShowByDoctor(doctor.Id);

        await unitOfWork.SaveChangesAsync(ct);
    }
}

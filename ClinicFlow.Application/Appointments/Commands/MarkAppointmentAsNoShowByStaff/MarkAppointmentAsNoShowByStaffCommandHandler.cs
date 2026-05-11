using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByStaff;

public sealed class MarkAppointmentAsNoShowByStaffCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<MarkAppointmentAsNoShowByStaffCommand>
{
    public async Task Handle(MarkAppointmentAsNoShowByStaffCommand request, CancellationToken ct)
    {
        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        appointment.MarkAsNoShowByStaff();

        await unitOfWork.SaveChangesAsync(ct);
    }
}

using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ClearStaffInternalNotesByStaff;

public sealed class ClearStaffInternalNotesByStaffCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ClearStaffInternalNotesByStaffCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        ClearStaffInternalNotesByStaffCommand request,
        CancellationToken cancellationToken
    )
    {
        var appointment = await appointmentRepository.GetByIdAsync(
            request.AppointmentId,
            cancellationToken
        )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        appointment.ClearStaffInternalNotes();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

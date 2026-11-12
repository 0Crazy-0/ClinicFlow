using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ClearReceptionistNotesByStaff;

public sealed class ClearReceptionistNotesByStaffCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ClearReceptionistNotesByStaffCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        ClearReceptionistNotesByStaffCommand request,
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

        appointment.ClearReceptionistNotes();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

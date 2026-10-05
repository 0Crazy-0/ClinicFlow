using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.UpdateStaffInternalNotesByStaff;

public sealed class UpdateStaffInternalNotesByStaffCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateStaffInternalNotesByStaffCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        UpdateStaffInternalNotesByStaffCommand request,
        CancellationToken cancellationToken
    )
    {
        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        appointment.UpdateStaffInternalNotes(request.Notes);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

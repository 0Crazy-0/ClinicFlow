using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.UpdateReceptionistNotesByStaff;

public sealed class UpdateReceptionistNotesByStaffCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateReceptionistNotesByStaffCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        UpdateReceptionistNotesByStaffCommand request,
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

        appointment.UpdateReceptionistNotes(request.Notes);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

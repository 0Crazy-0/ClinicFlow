using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Cancellation;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;

public sealed class CancelAppointmentByStaffCommandHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelAppointmentByStaffCommand>
{
    public async Task Handle(
        CancelAppointmentByStaffCommand request,
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

        AppointmentCancellationService.CancelByStaff(
            appointment,
            new StaffCancellationArgs
            {
                InitiatorUserId = request.InitiatorUserId,
                Reason = request.Reason,
                CancelledAt = timeProvider.GetUtcNow().UtcDateTime,
            }
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

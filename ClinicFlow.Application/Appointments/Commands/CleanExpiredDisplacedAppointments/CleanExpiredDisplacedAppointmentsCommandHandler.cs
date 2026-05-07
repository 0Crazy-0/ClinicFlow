using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CleanExpiredDisplacedAppointments;

public sealed class CleanExpiredDisplacedAppointmentsCommandHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CleanExpiredDisplacedAppointmentsCommand>
{
    public async Task Handle(
        CleanExpiredDisplacedAppointmentsCommand request,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiredAppointments = await appointmentRepository.GetExpiredDisplacedAppointmentsAsync(
            now,
            cancellationToken
        );

        foreach (var appointment in expiredAppointments)
            appointment.CancelDueToSystemTimeout(now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

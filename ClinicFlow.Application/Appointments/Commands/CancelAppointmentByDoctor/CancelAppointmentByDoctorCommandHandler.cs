using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Cancellation;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByDoctor;

public sealed class CancelAppointmentByDoctorCommandHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelAppointmentByDoctorCommand>
{
    public async Task Handle(
        CancelAppointmentByDoctorCommand request,
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

        var initiatorDoctor =
            await doctorRepository.GetByUserIdAsync(request.InitiatorUserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.InitiatorUserId
            );

        AppointmentCancellationService.CancelByDoctor(
            appointment,
            new DoctorCancellationArgs
            {
                InitiatorDoctorId = initiatorDoctor.Id,
                InitiatorUserId = request.InitiatorUserId,
                Reason = request.Reason,
                CancelledAt = timeProvider.GetUtcNow().UtcDateTime,
            }
        );

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

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
    public async Task Handle(CancelAppointmentByDoctorCommand request, CancellationToken ct)
    {
        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        var initiatorDoctor =
            await doctorRepository.GetByUserIdAsync(request.InitiatorUserId, ct)
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

        await unitOfWork.SaveChangesAsync(ct);
    }
}

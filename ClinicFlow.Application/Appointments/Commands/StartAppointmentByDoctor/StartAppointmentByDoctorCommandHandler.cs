using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.StartAppointmentByDoctor;

public sealed class StartAppointmentByDoctorCommandHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<StartAppointmentByDoctorCommand>
{
    public async Task Handle(
        StartAppointmentByDoctorCommand request,
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

        appointment.Start(initiatorDoctor.Id, timeProvider.GetUtcNow().UtcDateTime);

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

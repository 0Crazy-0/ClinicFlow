using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.NoShow;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShow;

public class MarkAppointmentAsNoShowCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUserRepository userRepository,
    IDoctorRepository doctorRepository,
    IPatientPenaltyRepository patientPenaltyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<MarkAppointmentAsNoShowCommand>
{
    public async Task Handle(
        MarkAppointmentAsNoShowCommand request,
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

        var initiator =
            await userRepository.GetByIdAsync(request.InitiatorUserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
                request.InitiatorUserId
            );

        var doctor = await doctorRepository.GetByUserIdAsync(initiator.Id, cancellationToken);

        var existingPenalties = await patientPenaltyRepository.GetByPatientIdAsync(
            appointment.PatientId,
            cancellationToken
        );

        var noShowArgs = new AppointmentNoShowArgs
        {
            InitiatorRole = initiator.Role,
            InitiatorDoctorId = doctor?.Id,
        };

        var newPenalties = AppointmentNoShowService.MarkAsNoShow(
            appointment,
            noShowArgs,
            existingPenalties
        );

        foreach (var penalty in newPenalties)
            await patientPenaltyRepository.AddAsync(penalty, cancellationToken);

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

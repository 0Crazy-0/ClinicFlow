using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShow;

public class MarkAppointmentAsNoShowCommandHandler(IAppointmentRepository appointmentRepository, IUserRepository userRepository,
    IPatientPenaltyRepository patientPenaltyRepository, IUnitOfWork unitOfWork) : IRequestHandler<MarkAppointmentAsNoShowCommand>
{
    public async Task Handle(MarkAppointmentAsNoShowCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId) ?? throw new EntityNotFoundException(nameof(Appointment), request.AppointmentId);

        var initiator = await userRepository.GetByIdAsync(request.InitiatorUserId) ?? throw new EntityNotFoundException(nameof(User), request.InitiatorUserId);

        var existingPenalties = await patientPenaltyRepository.GetByPatientIdAsync(appointment.PatientId);
        
        var newPenalties = AppointmentNoShowService.MarkAsNoShow(appointment, initiator, existingPenalties);

        foreach (var penalty in newPenalties)
            await patientPenaltyRepository.AddAsync(penalty);

        await appointmentRepository.UpdateAsync(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

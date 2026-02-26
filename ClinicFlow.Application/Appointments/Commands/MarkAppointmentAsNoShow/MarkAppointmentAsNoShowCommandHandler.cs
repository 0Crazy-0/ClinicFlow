using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
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

        if (initiator.Role is not (UserRole.Admin or UserRole.Receptionist))
        {
            if (initiator.Role is UserRole.Doctor)
            {
                if (initiator.DoctorId != appointment.DoctorId)
                    throw new AppointmentCancellationUnauthorizedException("Doctors can only mark their own appointments as No-Show.");

            }
            else
            {
                throw new AppointmentCancellationUnauthorizedException("User is not authorized to mark this appointment as No-Show.");
            }
        }

        appointment.MarkAsNoShow();

        var existingPenalties = await patientPenaltyRepository.GetByPatientIdAsync(appointment.PatientId);
        var newPenalties = PatientPenaltyService.ApplyPenalty(appointment.PatientId, existingPenalties, appointment.Id, "No show");

        foreach (var penalty in newPenalties)
            await patientPenaltyRepository.AddAsync(penalty);


        await appointmentRepository.UpdateAsync(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

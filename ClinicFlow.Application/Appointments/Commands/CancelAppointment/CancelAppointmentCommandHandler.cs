using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler(IAppointmentRepository appointmentRepository, IUserRepository userRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeDefinitionRepository, IDoctorRepository doctorRepository,
    IMedicalSpecialtyRepository medicalSpecialtyRepository, IPatientPenaltyRepository patientPenaltyRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelAppointmentCommand>
{
    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId) ?? throw new EntityNotFoundException(nameof(Appointment), request.AppointmentId);

        var initiator = await userRepository.GetByIdAsync(request.InitiatorUserId) ?? throw new EntityNotFoundException(nameof(User), request.InitiatorUserId);

        var appointmentType = await appointmentTypeDefinitionRepository.GetByIdAsync(appointment.AppointmentTypeId)
            ?? throw new EntityNotFoundException(nameof(AppointmentTypeDefinition), appointment.AppointmentTypeId);

        var doctor = await doctorRepository.GetByIdAsync(appointment.DoctorId) ?? throw new EntityNotFoundException(nameof(Doctor), appointment.DoctorId);

        var specialty = await medicalSpecialtyRepository.GetByIdAsync(doctor.MedicalSpecialtyId) ??
            throw new EntityNotFoundException(nameof(MedicalSpecialty), doctor.MedicalSpecialtyId);

        AppointmentCancellationService.CancelAppointment(appointment, initiator, appointmentType, request.IsAuthorizedFamilyMember, specialty, request.Reason);

        await appointmentRepository.UpdateAsync(appointment);

        if (appointment.Status is AppointmentStatus.LateCancellation)
        {
            var existingPenalties = await patientPenaltyRepository.GetByPatientIdAsync(appointment.PatientId);
            var newPenalties = PatientPenaltyService.ApplyPenalty(appointment.PatientId, existingPenalties, appointment.Id, "Late cancellation");

            foreach (var penalty in newPenalties)
                await patientPenaltyRepository.AddAsync(penalty);

        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

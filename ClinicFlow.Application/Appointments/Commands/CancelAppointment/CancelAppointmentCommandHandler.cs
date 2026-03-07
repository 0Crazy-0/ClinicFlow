using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler(IAppointmentRepository appointmentRepository, IUserRepository userRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeDefinitionRepository, IDoctorRepository doctorRepository,
    IMedicalSpecialtyRepository medicalSpecialtyRepository, IPatientPenaltyRepository patientPenaltyRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelAppointmentCommand>
{
    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken) ?? throw new EntityNotFoundException(nameof(Appointment), request.AppointmentId);

        var initiator = await userRepository.GetByIdAsync(request.InitiatorUserId, cancellationToken) ?? throw new EntityNotFoundException(nameof(User), request.InitiatorUserId);

        var appointmentType = await appointmentTypeDefinitionRepository.GetByIdAsync(appointment.AppointmentTypeId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(AppointmentTypeDefinition), appointment.AppointmentTypeId);

        var doctor = await doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken) ?? throw new EntityNotFoundException(nameof(Doctor), appointment.DoctorId);

        var specialty = await medicalSpecialtyRepository.GetByIdAsync(doctor.MedicalSpecialtyId, cancellationToken) ??
            throw new EntityNotFoundException(nameof(MedicalSpecialty), doctor.MedicalSpecialtyId);

        var context = new AppointmentCancellationContext
        {
            Initiator = initiator,
            AppointmentTypeDefinition = appointmentType,
            Specialty = specialty,
            IsAuthorizedFamilyMember = request.IsAuthorizedFamilyMember,
            Reason = request.Reason
        };

        AppointmentCancellationService.CancelAppointment(appointment, context);

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);

        if (appointment.Status is AppointmentStatus.LateCancellation)
        {
            var existingPenalties = await patientPenaltyRepository.GetByPatientIdAsync(appointment.PatientId, cancellationToken);
            var newPenalties = PatientPenaltyService.ApplyPenalty(appointment.PatientId, existingPenalties, appointment.Id, "Late cancellation");

            foreach (var penalty in newPenalties)
                await patientPenaltyRepository.AddAsync(penalty, cancellationToken);

        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

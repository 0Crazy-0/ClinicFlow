using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;

namespace ClinicFlow.Domain.Services;

public class AppointmentCancellationService(IMedicalSpecialtyRepository medicalSpecialtyRepository, IDoctorRepository doctorRepository)
{
    public async Task CancelAppointmentAsync(Appointment appointment, User initiator, AppointmentTypeDefinition appointmentTypeDefinition, bool isAuthorizedFamilyMember, string? reason)
    {
        ValidateCancellationPermission(appointment, initiator, appointmentTypeDefinition, isAuthorizedFamilyMember);
        ValidateCancellationReason(initiator.Role, reason);

        var doctor = await doctorRepository.GetByIdAsync(appointment.DoctorId) ?? throw new EntityNotFoundException(nameof(Doctor), appointment.DoctorId);

        var specialty = await medicalSpecialtyRepository.GetByIdAsync(doctor.MedicalSpecialtyId) ??
            throw new EntityNotFoundException(nameof(MedicalSpecialty), doctor.MedicalSpecialtyId);

        appointment.Cancel(initiator.Id, reason, specialty);
    }

    private static void ValidateCancellationPermission(Appointment appointment, User initiator, AppointmentTypeDefinition appointmentTypeDefinition, bool isFamilyMember)
    {
        if (initiator.Role is UserRole.Admin or UserRole.Receptionist) return;

        if (initiator.Role is UserRole.Doctor)
        {
            if (initiator.DoctorId == appointment.DoctorId) return;

            throw new AppointmentCancellationUnauthorizedException("Doctors can only cancel their own appointments.");
        }

        if (initiator.Role is UserRole.Patient)
        {
            if (initiator.PatientId == appointment.PatientId) return;

            if (isFamilyMember)
            {
                var allowedFamilyTypes = new[]
                {
                    AppointmentType.Checkup,
                    AppointmentType.FollowUp,
                    AppointmentType.FirstConsultation
                };

                if (allowedFamilyTypes.Contains(appointmentTypeDefinition.Type)) return;

                throw new AppointmentCancellationUnauthorizedException($"Family members cannot cancel appointments of type: {appointmentTypeDefinition.Type}");
            }
        }
        throw new AppointmentCancellationUnauthorizedException("User is not authorized to cancel this appointment.");
    }

    // Helper
    private static void ValidateCancellationReason(UserRole role, string? reason)
    {
        bool isStaff = role is UserRole.Admin or UserRole.Receptionist;

        if (isStaff && string.IsNullOrWhiteSpace(reason)) throw new BusinessRuleValidationException("Staff members must provide a reason for cancellation.");
    }
}

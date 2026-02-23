using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service that orchestrates appointment cancellation, enforcing authorization
/// and business rules such as required cancellation reasons for staff members.
/// </summary>
public static class AppointmentCancellationService
{
    /// <summary>
    /// Cancels an appointment after validating the initiator's permission and any required cancellation reason.
    /// </summary>
    /// <param name="isAuthorizedFamilyMember">Indicates whether the initiator is an authorized family member of the patient.</param>
    /// <param name="specialty">The medical specialty, used to evaluate the cancellation notice policy.</param>
    /// <exception cref="AppointmentCancellationUnauthorizedException">Thrown when the initiator is not authorized to cancel the appointment.</exception>
    /// <exception cref="BusinessRuleValidationException">Thrown when a staff member does not provide a cancellation reason.</exception>
    public static void CancelAppointment(Appointment appointment, User initiator, AppointmentTypeDefinition appointmentTypeDefinition, bool isAuthorizedFamilyMember,
        MedicalSpecialty specialty, string? reason)
    {
        ValidateCancellationPermission(appointment, initiator, appointmentTypeDefinition, isAuthorizedFamilyMember);
        ValidateCancellationReason(initiator.Role, reason);

        appointment.Cancel(initiator.Id, reason, specialty);
    }

    // Helpers
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

    private static void ValidateCancellationReason(UserRole role, string? reason)
    {
        bool isStaff = role is UserRole.Admin or UserRole.Receptionist;

        if (isStaff && string.IsNullOrWhiteSpace(reason)) throw new BusinessRuleValidationException("Staff members must provide a reason for cancellation.");
    }
}

using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Contexts;

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
    public static void CancelAppointment(Appointment appointment, AppointmentCancellationContext context)
    {
        ValidateCancellationPermission(appointment, context.Initiator, context.InitiatorDoctorId, context.InitiatorPatientId, context.AppointmentTypeDefinition,
            context.IsAuthorizedFamilyMember);

        ValidateCancellationReason(context.Initiator.Role, context.Reason);

        appointment.Cancel(context.Initiator.Id, context.Reason, context.Specialty);
    }

    // Helpers
    private static void ValidateCancellationPermission(Appointment appointment, User initiator, Guid? initiatorDoctorId, Guid? initiatorPatientId, 
        AppointmentTypeDefinition appointmentTypeDefinition, bool isFamilyMember)
    {
        if (initiator.Role is UserRole.Admin or UserRole.Receptionist) return;

        if (initiator.Role is UserRole.Doctor)
        {
            if (!initiatorDoctorId.HasValue)
                throw new DomainValidationException("A user with the Doctor role must have an associated doctor profile.");

            if (initiatorDoctorId == appointment.DoctorId) return;

            throw new AppointmentCancellationUnauthorizedException("Doctors can only cancel their own appointments.");
        }

        if (initiator.Role is UserRole.Patient)
        {
            if (!initiatorPatientId.HasValue)
                throw new DomainValidationException("A user with the Patient role must have an associated patient profile.");

            if (initiatorPatientId == appointment.PatientId) return;

            if (isFamilyMember)
            {
                var allowedFamilyTypes = new[]
                {
                    AppointmentCategory.Checkup,
                    AppointmentCategory.FollowUp,
                    AppointmentCategory.FirstConsultation
                };

                if (allowedFamilyTypes.Contains(appointmentTypeDefinition.Category)) return;

                throw new AppointmentCancellationUnauthorizedException($"Family members cannot cancel appointments of type: {appointmentTypeDefinition.Category}");
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

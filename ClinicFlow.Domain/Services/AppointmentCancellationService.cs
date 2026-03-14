using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Common;

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
    /// <param name="appointment">The appointment to be canceled.</param>
    /// <param name="context">The context containing necessary details for the cancellation, such as the initiator's role, specialty, and reason.</param>
    /// <exception cref="AppointmentCancellationUnauthorizedException">Thrown when the initiator is not authorized to cancel the appointment.</exception>
    /// <exception cref="BusinessRuleValidationException">Thrown when a staff member does not provide a cancellation reason.</exception>
    public static void CancelAppointment(Appointment appointment, AppointmentCancellationContext context)
    {
        ValidateCancellationPermission(appointment, context);

        ValidateCancellationReason(context.Initiator.Role, context.Reason);

        appointment.Cancel(context.Initiator.Id, context.Reason, context.Specialty);
    }

    // Helpers
    private static void ValidateCancellationPermission(Appointment appointment, AppointmentCancellationContext context)
    {
        switch (context.Initiator.Role)
        {
            case UserRole.Admin or UserRole.Receptionist:
                return;
            case UserRole.Doctor:
                ValidateDoctorCancellationPermission(appointment, context.InitiatorDoctorId);
                return;
            case UserRole.Patient:
                ValidatePatientCancellationPermission(appointment, context.InitiatorPatientId, context.AppointmentTypeDefinition, context.IsAuthorizedFamilyMember);
                return;
            default:
                throw new AppointmentCancellationUnauthorizedException(DomainErrors.Appointment.CannotCancel);
        }
    }

    private static void ValidateDoctorCancellationPermission(Appointment appointment, Guid? initiatorDoctorId)
    {
        if (!initiatorDoctorId.HasValue) throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (initiatorDoctorId != appointment.DoctorId) throw new AppointmentCancellationUnauthorizedException(DomainErrors.Appointment.UnauthorizedCancellation);
    }

    private static void ValidatePatientCancellationPermission(Appointment appointment, Guid? initiatorPatientId,
        AppointmentTypeDefinition appointmentTypeDefinition, bool isFamilyMember)
    {
        if (!initiatorPatientId.HasValue) throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (initiatorPatientId == appointment.PatientId) return;

        if (!isFamilyMember) throw new AppointmentCancellationUnauthorizedException(DomainErrors.Appointment.UnauthorizedCancellation);

        ValidateFamilyMemberCancellationPermission(appointmentTypeDefinition);
    }

    private static void ValidateFamilyMemberCancellationPermission(AppointmentTypeDefinition appointmentTypeDefinition)
    {
        var allowedFamilyTypes = new[]
        {
            AppointmentCategory.Checkup,
            AppointmentCategory.FollowUp,
            AppointmentCategory.FirstConsultation
        };

        if (!allowedFamilyTypes.Contains(appointmentTypeDefinition.Category)) 
            throw new AppointmentCancellationUnauthorizedException(DomainErrors.Appointment.UnauthorizedCancellation);
    }

    private static void ValidateCancellationReason(UserRole role, string? reason)
    {
        bool isStaff = role is UserRole.Admin or UserRole.Receptionist;

        if (isStaff && string.IsNullOrWhiteSpace(reason)) throw new BusinessRuleValidationException(DomainErrors.Appointment.MissingCancellationReason);
    }
}

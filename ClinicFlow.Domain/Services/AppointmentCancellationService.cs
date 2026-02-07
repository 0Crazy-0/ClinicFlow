using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions;

namespace ClinicFlow.Domain.Services;

public class AppointmentCancellationService
{
    public void CancelAppointment(Appointment appointment, User initiator, AppointmentType appointmentType, bool isAuthorizedFamilyMember, string? reason)
    {
        ValidateCancellationPermission(appointment, initiator, appointmentType, isAuthorizedFamilyMember);
        ValidateCancellationReason(initiator.Role, reason);

        appointment.Cancel(initiator.Id, reason, appointmentType.Type);
    }

    private static void ValidateCancellationPermission(Appointment appointment, User initiator, AppointmentType appointmentType, bool isFamilyMember)
    {
        if (initiator.Role is UserRoleEnum.Admin or UserRoleEnum.Receptionist) return;

        if (initiator.Role is UserRoleEnum.Doctor)
        {
            if (initiator.DoctorId == appointment.DoctorId) return;

            throw new AppointmentCancellationUnauthorizedException("Doctors can only cancel their own appointments.");
        }

        if (initiator.Role is UserRoleEnum.Patient)
        {
            if (initiator.PatientId == appointment.PatientId) return;

            if (isFamilyMember)
            {
                var allowedFamilyTypes = new[]
                {
                    AppointmentTypeEnum.Checkup,
                    AppointmentTypeEnum.FollowUp,
                    AppointmentTypeEnum.FirstConsultation
                };

                if (allowedFamilyTypes.Contains(appointmentType.Type)) return;

                throw new AppointmentCancellationUnauthorizedException($"Family members cannot cancel appointments of type: {appointmentType.Type}");
            }
        }
        throw new AppointmentCancellationUnauthorizedException("User is not authorized to cancel this appointment.");
    }

    private static void ValidateCancellationReason(UserRoleEnum role, string? reason)
    {
        bool isStaff = role is UserRoleEnum.Admin or UserRoleEnum.Receptionist;

        if (isStaff && string.IsNullOrWhiteSpace(reason)) throw new BusinessRuleValidationException("Staff members must provide a reason for cancellation.");
    }
}

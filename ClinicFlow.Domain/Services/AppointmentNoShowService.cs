using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service that orchestrates marking an appointment as a no-show, enforcing authorization
/// and business rules such as applying penalties to the patient.
/// </summary>
public static class AppointmentNoShowService
{
    /// <summary>
    /// Marks an appointment as a no-show after validating the initiator's permission, and returns
    /// any new penalties that should be applied to the patient.
    /// </summary>
    /// <param name="appointment">The appointment to mark as a no-show.</param>
    /// <param name="initiatorRole">The role of the user attempting to mark the appointment as a no-show.</param>
    /// <param name="initiatorDoctorId">The doctor ID of the initiator, if applicable.</param>
    /// <param name="existingPenalties">The existing penalties for the patient, used to determine if a block should be applied.</param>
    /// <returns>A collection of new penalties to be saved.</returns>
    /// <exception cref="AppointmentCancellationUnauthorizedException">Thrown when the initiator is not authorized to mark the appointment as a no-show.</exception>
    public static IEnumerable<PatientPenalty> MarkAsNoShow(Appointment appointment, UserRole initiatorRole, Guid? initiatorDoctorId, IEnumerable<PatientPenalty> existingPenalties)
    {
        ValidateNoShowPermission(appointment.DoctorId, initiatorRole, initiatorDoctorId);

        appointment.MarkAsNoShow();

        return PatientPenaltyService.ApplyPenalty(appointment.PatientId, existingPenalties, appointment.Id, "No show");
    }

    private static void ValidateNoShowPermission(Guid appointmentDoctorId, UserRole initiatorRole, Guid? initiatorDoctorId)
    {
        if (initiatorRole is not (UserRole.Admin or UserRole.Receptionist))
        {
            if (initiatorRole is UserRole.Doctor)
            {
                if (!initiatorDoctorId.HasValue)
                    throw new DomainValidationException("A user with the Doctor role must have an associated doctor profile.");

                if (initiatorDoctorId != appointmentDoctorId)
                    throw new AppointmentCancellationUnauthorizedException("Doctors can only mark their own appointments as No-Show.");
            }
            else
            {
                throw new AppointmentCancellationUnauthorizedException("User is not authorized to mark this appointment as No-Show.");
            }
        }
    }
}

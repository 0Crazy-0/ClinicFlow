using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Services.Args.NoShow;

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
    /// <param name="args">The arguments containing the initiator's role and doctor ID.</param>
    /// <param name="existingPenalties">The existing penalties for the patient, used to determine if a block should be applied.</param>
    /// <returns>A collection of new penalties to be saved.</returns>
    /// <exception cref="AppointmentNoShowUnauthorizedException">Thrown when the initiator is not authorized to mark the appointment as a no-show.</exception>
    public static IEnumerable<PatientPenalty> MarkAsNoShow(
        Appointment appointment,
        AppointmentNoShowArgs args,
        IEnumerable<PatientPenalty> existingPenalties
    )
    {
        ValidateNoShowPermission(appointment.DoctorId, args.InitiatorRole, args.InitiatorDoctorId);

        appointment.MarkAsNoShow();

        return PatientPenaltyService.ApplyPenalty(
            appointment.PatientId,
            existingPenalties,
            appointment.Id,
            PenaltyReasons.NoShow
        );
    }

    private static void ValidateNoShowPermission(
        Guid appointmentDoctorId,
        UserRole initiatorRole,
        Guid? initiatorDoctorId
    )
    {
        if (initiatorRole is UserRole.Admin or UserRole.Receptionist)
            return;

        if (
            initiatorRole is UserRole.Doctor
            && initiatorDoctorId.HasValue
            && initiatorDoctorId == appointmentDoctorId
        )
            return;

        throw new AppointmentNoShowUnauthorizedException(DomainErrors.Appointment.CannotMarkNoShow);
    }
}

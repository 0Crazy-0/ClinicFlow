using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Cancellation;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service responsible for enforcing invariant rules when cancelling an appointment.
/// </summary>
public static class AppointmentCancellationService
{
    /// <summary>
    /// Cancels an appointment on behalf of a patient, enforcing specific domain invariants.
    /// </summary>
    /// <param name="appointment">The appointment to be cancelled.</param>
    /// <param name="args">The cancellation arguments containing the target patient, the initiator patient,category, specialty, and reason.</param>
    public static void CancelByPatient(Appointment appointment, PatientCancellationArgs args)
    {
        if (appointment.PatientId != args.TargetPatient.Id)
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        if (args.InitiatorPatient is null)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (args.TargetPatient.UserId != args.InitiatorPatient.UserId)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedCancellation
            );

        if (args.Category is AppointmentCategory.Procedure)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.CannotCancel
            );

        if (args.Category is AppointmentCategory.Emergency)
            ValidateEmergencyCancellation(args.TargetPatient, args.CancelledAt);

        appointment.Cancel(
            args.InitiatorPatient.UserId,
            args.Reason,
            args.Specialty,
            args.CancelledAt
        );
    }

    /// <summary>
    /// Cancels an appointment on behalf of a doctor.
    /// </summary>
    /// <param name="appointment">The appointment to be cancelled.</param>
    /// <param name="args">The cancellation arguments containing the initiator doctor, specialty, and reason.</param>
    public static void CancelByDoctor(Appointment appointment, DoctorCancellationArgs args)
    {
        if (args.InitiatorDoctor is null)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (args.InitiatorDoctor.Id != appointment.DoctorId)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedCancellation
            );

        appointment.Cancel(
            args.InitiatorDoctor.UserId,
            args.Reason,
            args.Specialty,
            args.CancelledAt,
            true
        );
    }

    /// <summary>
    /// Cancels an appointment on behalf of a staff member. A cancellation reason is strictly required.
    /// </summary>
    /// <param name="appointment">The appointment to be cancelled.</param>
    /// <param name="args">The cancellation arguments containing the initiator user, specialty, and required reason.</param>
    public static void CancelByStaff(Appointment appointment, StaffCancellationArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.Reason))
            throw new BusinessRuleValidationException(
                DomainErrors.Appointment.MissingCancellationReason
            );

        appointment.Cancel(
            args.InitiatorUserId,
            args.Reason,
            args.Specialty,
            args.CancelledAt,
            true
        );
    }

    /// <summary>
    /// Validates if an emergency appointment can be cancelled based on the relationship between the patient and the initiator user.
    /// Emergency appointments can only be cancelled by the patients themselves or by a parent if the patient is under 18.
    /// </summary>
    /// <param name="patient">The patient associated with the emergency appointment.</param>
    private static void ValidateEmergencyCancellation(Patient patient, DateTime referenceTime)
    {
        if (patient.RelationshipToUser is PatientRelationship.Self)
            return;

        if (
            patient.RelationshipToUser is PatientRelationship.Child
            && patient.GetAge(referenceTime) < 18
        )
            return;

        throw new AppointmentCancellationUnauthorizedException(
            DomainErrors.Appointment.CannotCancel
        );
    }
}

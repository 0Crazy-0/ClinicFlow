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
    /// <param name="args">The cancellation arguments containing the appointment patient, the initiator patient,category, specialty, and reason.</param>
    /// <exception cref="DomainValidationException">
    /// Thrown if <see cref="PatientCancellationArgs.AppointmentPatient"/> does not match the
    /// appointment's patient, or if <see cref="PatientCancellationArgs.InitiatorPatient"/> is null.
    /// </exception>
    /// <exception cref="AppointmentCancellationUnauthorizedException">
    /// Thrown if the <see cref="PatientCancellationArgs.InitiatorPatient"/>'s associated user does not
    /// match the <see cref="PatientCancellationArgs.AppointmentPatient"/>'s user, if the appointment
    /// category is <see cref="AppointmentCategory.Procedure"/>, or if it is an
    /// <see cref="AppointmentCategory.Emergency"/> and the emergency cancellation rules are not met.
    /// </exception>
    /// <exception cref="AppointmentCancellationNotAllowedException"> Thrown if the appointment is already cancelled.</exception>
    public static void CancelByPatient(Appointment appointment, PatientCancellationArgs args)
    {
        if (appointment.PatientId != args.AppointmentPatient.Id)
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        if (args.InitiatorPatient is null)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (args.AppointmentPatient.UserId != args.InitiatorPatient.UserId)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedCancellation
            );

        if (args.Category is AppointmentCategory.Procedure)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.CannotCancel
            );

        if (args.Category is AppointmentCategory.Emergency)
            ValidateEmergencyCancellation(args.AppointmentPatient);

        appointment.Cancel(args.InitiatorPatient.UserId, args.Reason, args.Specialty);
    }

    /// <summary>
    /// Cancels an appointment on behalf of a doctor.
    /// </summary>
    /// <param name="appointment">The appointment to be cancelled.</param>
    /// <param name="args">The cancellation arguments containing the initiator doctor, specialty, and reason.</param>
    /// <exception cref="DomainValidationException">Thrown if the initiator doctor is null.</exception>
    /// <exception cref="AppointmentCancellationUnauthorizedException">Thrown if the initiator doctor is not the doctor assigned to the appointment.</exception>
    /// <exception cref="AppointmentCancellationNotAllowedException">Thrown if the appointment is already cancelled.</exception>
    public static void CancelByDoctor(Appointment appointment, DoctorCancellationArgs args)
    {
        if (args.InitiatorDoctor is null)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (args.InitiatorDoctor.Id != appointment.DoctorId)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedCancellation
            );

        appointment.Cancel(args.InitiatorDoctor.UserId, args.Reason, args.Specialty, true);
    }

    /// <summary>
    /// Cancels an appointment on behalf of a staff member. A cancellation reason is strictly required.
    /// </summary>
    /// <param name="appointment">The appointment to be cancelled.</param>
    /// <param name="args">The cancellation arguments containing the initiator user, specialty, and required reason.</param>
    /// <exception cref="BusinessRuleValidationException">Thrown if the cancellation reason is missing or consists only of whitespace.</exception>
    /// <exception cref="AppointmentCancellationNotAllowedException">Thrown if the appointment is already cancelled.</exception>
    public static void CancelByStaff(Appointment appointment, StaffCancellationArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.Reason))
            throw new BusinessRuleValidationException(
                DomainErrors.Appointment.MissingCancellationReason
            );

        appointment.Cancel(args.InitiatorUserId, args.Reason, args.Specialty, true);
    }

    /// <summary>
    /// Validates if an emergency appointment can be cancelled based on the relationship between the patient and the initiator user.
    /// Emergency appointments can only be cancelled by the patients themselves or by a parent if the patient is under 18.
    /// </summary>
    /// <param name="patient">The patient associated with the emergency appointment.</param>
    /// <exception cref="AppointmentCancellationUnauthorizedException">Thrown if the relationship conditions are not met.</exception>
    private static void ValidateEmergencyCancellation(Patient patient)
    {
        if (patient.RelationshipToUser is PatientRelationship.Self)
            return;

        if (patient.RelationshipToUser is PatientRelationship.Child && patient.GetAge() < 18)
            return;

        throw new AppointmentCancellationUnauthorizedException(
            DomainErrors.Appointment.CannotCancel
        );
    }
}

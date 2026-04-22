using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Cancellation;
using ClinicFlow.Domain.Services.Contexts;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service responsible for enforcing invariant rules when cancelling an appointment.
/// </summary>
public static class AppointmentCancellationService
{
    /// <summary>
    /// Executes patient-initiated cancellation.
    /// </summary>
    /// <remarks>
    /// Enforces authorization, restricts procedure cancellations, and automatically applies late cancellation if notice period is insufficient.
    /// </remarks>
    public static void CancelByPatient(
        Appointment appointment,
        AppointmentCancellationContext context,
        PatientCancellationArgs args
    )
    {
        if (appointment.PatientId != args.TargetPatient.Id)
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        if (args.InitiatorPatient is null)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (args.TargetPatient.UserId != args.InitiatorPatient.UserId)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedCancellation
            );

        if (context.Category is AppointmentCategory.Procedure)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.CannotCancel
            );

        if (context.Category is AppointmentCategory.Emergency)
            ValidateEmergencyCancellation(args.TargetPatient, args.CancelledAt);

        if (
            context.Specialty.IsCancellationAllowed(
                appointment.ScheduledDate.Add(appointment.TimeRange.Start),
                args.CancelledAt
            )
        )
        {
            appointment.Cancel(args.InitiatorPatient.UserId, args.Reason, args.CancelledAt);
        }
        else
        {
            appointment.CancelLate(args.InitiatorPatient.UserId, args.Reason, args.CancelledAt);
        }
    }

    public static void CancelByDoctor(Appointment appointment, DoctorCancellationArgs args)
    {
        if (args.InitiatorDoctor is null)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (args.InitiatorDoctor.Id != appointment.DoctorId)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedCancellation
            );

        appointment.Cancel(args.InitiatorDoctor.UserId, args.Reason, args.CancelledAt);
    }

    public static void CancelByStaff(Appointment appointment, StaffCancellationArgs args)
    {
        if (string.IsNullOrWhiteSpace(args.Reason))
            throw new BusinessRuleValidationException(
                DomainErrors.Appointment.MissingCancellationReason
            );

        appointment.Cancel(args.InitiatorUserId, args.Reason, args.CancelledAt);
    }

    /// <summary>
    /// Validates if an emergency appointment can be cancelled based on the relationship between the patient and the initiator user.
    /// </summary>
    /// <remarks>
    /// Emergency appointments can only be cancelled by the patients themselves or by a parent if the patient is under 18.
    /// </remarks>
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

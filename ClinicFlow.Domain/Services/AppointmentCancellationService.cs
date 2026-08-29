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
    public static void CancelByPatient(
        Appointment appointment,
        AppointmentCancellationContext context,
        PatientCancellationArgs args
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Specialty);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.TargetPatient);

        if (appointment.PatientId != args.TargetPatient.Id)
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        if (context.Category is AppointmentCategory.Procedure)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.CannotCancel
            );

        if (context.Category is AppointmentCategory.Emergency)
            ValidateEmergencyCancellation(
                args.TargetPatient,
                context.IsInitiatorSelfOfTarget,
                context.IsInitiatorGuardianOfMinorTarget,
                DateOnly.FromDateTime(args.CancelledAt)
            );

        if (
            context.Specialty.IsCancellationAllowed(
                appointment.ScheduledDate.ToDateTime(appointment.TimeRange.Start),
                args.CancelledAt
            )
        )
        {
            appointment.Cancel(
                args.InitiatorUserId,
                args.Reason,
                DateOnly.FromDateTime(args.CancelledAt)
            );
        }
        else
        {
            appointment.CancelLate(
                args.InitiatorUserId,
                args.Reason,
                DateOnly.FromDateTime(args.CancelledAt)
            );
        }
    }

    public static void CancelByDoctor(Appointment appointment, DoctorCancellationArgs args)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);

        if (args.InitiatorDoctorId != appointment.DoctorId)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedCancellation
            );

        appointment.Cancel(
            args.InitiatorUserId,
            args.Reason,
            DateOnly.FromDateTime(args.CancelledAt)
        );
    }

    public static void CancelByStaff(Appointment appointment, StaffCancellationArgs args)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.Reason);

        if (string.IsNullOrWhiteSpace(args.Reason))
            throw new BusinessRuleValidationException(
                DomainErrors.Appointment.MissingCancellationReason
            );

        appointment.Cancel(
            args.InitiatorUserId,
            args.Reason,
            DateOnly.FromDateTime(args.CancelledAt)
        );
    }

    /// <summary>
    /// Validates if an emergency appointment can be cancelled based on the relationship between the patient and the initiator user.
    /// </summary>
    private static void ValidateEmergencyCancellation(
        Patient patient,
        bool isInitiatorSelfOfTarget,
        bool isInitiatorGuardianOfMinorTarget,
        DateOnly referenceDate
    )
    {
        if (isInitiatorSelfOfTarget)
            return;

        if (isInitiatorGuardianOfMinorTarget && patient.GetAge(referenceDate) < 18)
            return;

        throw new AppointmentCancellationUnauthorizedException(
            DomainErrors.Appointment.CannotCancel
        );
    }
}

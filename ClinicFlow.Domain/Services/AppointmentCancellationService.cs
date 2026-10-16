using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Cancellation;
using ClinicFlow.Domain.Services.Contexts.Cancellation;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service responsible for enforcing invariant rules when cancelling an appointment.
/// </summary>
public static class AppointmentCancellationService
{
    public static void CancelByPatient(
        Appointment appointment,
        PatientCancellationArgs args,
        AppointmentCancellationContext context
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.AppointmentType);
        ArgumentNullException.ThrowIfNull(context.Specialty);
        ArgumentNullException.ThrowIfNull(context.InitiatorMembership);
        ArgumentNullException.ThrowIfNull(args);

        context.InitiatorMembership.EnsureAppointmentAccess(context.AppointmentType.Category);

        if (appointment.PatientId != context.TargetPatient.Id)
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        if (context.AppointmentType.Purpose is AppointmentPurpose.Procedure)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.CannotCancel
            );

        if (context.AppointmentType.Purpose is AppointmentPurpose.Emergency)
            ValidateEmergencyCancellation(
                context.TargetPatient,
                context.InitiatorMembership,
                DateOnly.FromDateTime(context.CancelledAt)
            );

        if (
            context.Specialty.IsCancellationAllowed(
                appointment.ScheduledDate.ToDateTime(appointment.TimeSlot.Start),
                context.CancelledAt
            )
        )
        {
            appointment.Cancel(
                context.InitiatorMembership.UserId,
                args.Reason,
                DateOnly.FromDateTime(context.CancelledAt)
            );
        }
        else
        {
            appointment.CancelLate(
                context.InitiatorMembership.UserId,
                args.Reason,
                DateOnly.FromDateTime(context.CancelledAt)
            );
        }
    }

    public static void CancelByDoctor(
        Appointment appointment,
        DoctorCancellationArgs args,
        DoctorCancellationContext context
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.InitiatorDoctor);

        if (context.InitiatorDoctor.Id != appointment.DoctorId)
            throw new AppointmentCancellationUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedCancellation
            );

        appointment.Cancel(
            context.InitiatorDoctor.UserId,
            args.Reason,
            DateOnly.FromDateTime(context.CancelledAt)
        );
    }

    public static void CancelByStaff(
        Appointment appointment,
        StaffCancellationArgs args,
        StaffCancellationContext context
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.Reason);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(args.Reason))
            throw new BusinessRuleValidationException(
                DomainErrors.Appointment.MissingCancellationReason
            );

        appointment.Cancel(
            context.InitiatorUserId,
            args.Reason,
            DateOnly.FromDateTime(context.CancelledAt)
        );
    }

    /// <summary>
    /// Validates if an emergency appointment can be cancelled based on the relationship between the patient and the initiator user.
    /// </summary>
    private static void ValidateEmergencyCancellation(
        Patient patient,
        FamilyMembership initiatorMembership,
        DateOnly referenceDate
    )
    {
        if (initiatorMembership.Role is PatientRelationship.Self)
            return;

        if (
            initiatorMembership.LegalAuthority is not LegalAuthorityType.None
            && patient.GetAge(referenceDate) < DomainRules.AdultAge
        )
            return;

        throw new AppointmentCancellationUnauthorizedException(
            DomainErrors.Appointment.CannotCancel
        );
    }
}

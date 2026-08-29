using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Orchestrates appointment rescheduling for different actors (Patient, Doctor, Staff).
/// </summary>
/// <remarks>
/// All rescheduling requests must be accompanied by a valid scheduling clearance to ensure compliance with regional scheduling regulations.
/// </remarks>
public static class AppointmentReschedulingService
{
    public static void RescheduleByPatient(
        Appointment appointment,
        PatientReschedulingArgs args,
        PatientReschedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.TargetPatient);
        ArgumentNullException.ThrowIfNull(args.NewTimeRange);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Reschedule.MissingClearance);

        if (args.TargetPatient.Id != appointment.PatientId)
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        PatientAccessService.VerifyAccess(context.InitiatorHasAccessToTarget);

        if (!args.IsInitiatorPhoneVerified)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.PhoneNotVerified
            );

        new PenaltyHistory(context.Penalties).EnsureNotBlocked(args.NewDate);

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            appointment.DoctorId,
            args.NewDate.DayOfWeek,
            args.NewTimeRange
        );

        appointment.Reschedule(args.NewDate, args.NewTimeRange);

        if (args.NewPatientNotes is not null)
            appointment.UpdatePatientNotes(args.NewPatientNotes);
    }

    public static void RescheduleByDoctor(
        Appointment appointment,
        DoctorReschedulingArgs args,
        Schedule doctorSchedule,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.InitiatorDoctor);
        ArgumentNullException.ThrowIfNull(args.NewTimeRange);
        ArgumentNullException.ThrowIfNull(doctorSchedule);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Reschedule.MissingClearance);

        if (args.InitiatorDoctor.Id != appointment.DoctorId)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedScheduling
            );

        if (!args.IsOverbook)
            doctorSchedule.EnsureDoctorIsAvailable(
                appointment.DoctorId,
                args.NewDate.DayOfWeek,
                args.NewTimeRange
            );

        appointment.Reschedule(args.NewDate, args.NewTimeRange);
    }

    public static void RescheduleByStaff(
        Appointment appointment,
        StaffReschedulingArgs args,
        Schedule doctorSchedule,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.NewTimeRange);
        ArgumentNullException.ThrowIfNull(doctorSchedule);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Reschedule.MissingClearance);

        if (!args.IsOverbook)
            doctorSchedule.EnsureDoctorIsAvailable(
                appointment.DoctorId,
                args.NewDate.DayOfWeek,
                args.NewTimeRange
            );

        appointment.Reschedule(args.NewDate, args.NewTimeRange);
    }
}

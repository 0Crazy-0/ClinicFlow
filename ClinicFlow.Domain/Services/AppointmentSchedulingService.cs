using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Orchestrates appointment scheduling for different actors (Patient, Guardian, Doctor, Staff).
/// </summary>
/// <remarks>
/// All scheduling requests must be accompanied by a valid scheduling clearance to ensure compliance with regional scheduling regulations.
/// </remarks>
public static class AppointmentSchedulingService
{
    public static Appointment ScheduleByPatient(
        PatientSchedulingArgs args,
        PatientSchedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.RequestedTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.AppointmentType);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.InitiatorMembership);
        ArgumentNullException.ThrowIfNull(context.ClinicTimeZone);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        DaylightTransitionPolicy.EnsureSchedulable(
            args.ScheduledDate,
            args.RequestedTimeSlot,
            context.ClinicTimeZone
        );

        context.InitiatorMembership.EnsureAppointmentAccess(context.AppointmentType.Category);

        if (!context.IsInitiatorPhoneVerified)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.PhoneNotVerified
            );

        context.TargetPatient.EnsureCompleteProfile();

        new PenaltyHistory(context.Penalties).EnsureNotBlocked(args.ScheduledDate);

        context.AppointmentType.ValidatePatientEligibility(
            context.TargetPatient.GetAge(args.ScheduledDate)
        );

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            args.RequestedTimeSlot
        );

        var appointment = Appointment.Schedule(
            context.TargetPatient.Id,
            args.DoctorId,
            context.AppointmentType.Id,
            args.ScheduledDate,
            args.RequestedTimeSlot,
            context.InitiatorMembership.UserId
        );

        if (args.PatientNotes is not null)
            appointment.UpdatePatientNotes(args.PatientNotes);

        return appointment;
    }

    public static Appointment ScheduleByGuardian(
        GuardianSchedulingArgs args,
        PatientSchedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.RequestedTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.AppointmentType);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.InitiatorMembership);
        ArgumentNullException.ThrowIfNull(context.ClinicTimeZone);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        DaylightTransitionPolicy.EnsureSchedulable(
            args.ScheduledDate,
            args.RequestedTimeSlot,
            context.ClinicTimeZone
        );

        context.InitiatorMembership.EnsureGuardianAccess();

        if (!context.IsInitiatorPhoneVerified)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.PhoneNotVerified
            );

        context.TargetPatient.EnsureCompleteProfile();

        new PenaltyHistory(context.Penalties).EnsureNotBlocked(args.ScheduledDate);

        context.AppointmentType.ValidatePatientEligibility(
            context.TargetPatient.GetAge(args.ScheduledDate),
            true
        );

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            args.RequestedTimeSlot
        );

        var isMinorAtScheduledDate =
            context.TargetPatient.GetAge(args.ScheduledDate) < FamilyMembership.MinimumAdultAge;

        if (!isMinorAtScheduledDate)
            throw new DomainValidationException(DomainErrors.Appointment.GuardianRequiresMinor);

        var appointment = Appointment.Schedule(
            context.TargetPatient.Id,
            args.DoctorId,
            context.AppointmentType.Id,
            args.ScheduledDate,
            args.RequestedTimeSlot,
            context.InitiatorMembership.UserId
        );

        if (args.GuardianNotes is not null)
            appointment.UpdateGuardianNotes(args.GuardianNotes, context.InitiatorMembership.UserId);

        return appointment;
    }

    public static Appointment ScheduleByDoctor(
        DoctorSchedulingArgs args,
        DoctorSchedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.RequestedTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.InitiatorDoctor);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.AppointmentType);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.ClinicTimeZone);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        DaylightTransitionPolicy.EnsureSchedulable(
            args.ScheduledDate,
            args.RequestedTimeSlot,
            context.ClinicTimeZone
        );

        context.AppointmentType.ValidatePatientEligibility(
            context.TargetPatient.GetAge(args.ScheduledDate),
            args.HasGuardianConsentVerified
        );

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            context.InitiatorDoctor.Id,
            args.ScheduledDate.DayOfWeek,
            args.RequestedTimeSlot
        );

        return Appointment.Schedule(
            context.TargetPatient.Id,
            context.InitiatorDoctor.Id,
            context.AppointmentType.Id,
            args.ScheduledDate,
            args.RequestedTimeSlot,
            context.InitiatorDoctor.UserId
        );
    }

    public static Appointment ScheduleByStaff(
        StaffSchedulingArgs args,
        StaffSchedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.RequestedTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.AppointmentType);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.ClinicTimeZone);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        DaylightTransitionPolicy.EnsureSchedulable(
            args.ScheduledDate,
            args.RequestedTimeSlot,
            context.ClinicTimeZone
        );

        context.TargetPatient.EnsureCompleteProfile();

        context.AppointmentType.ValidatePatientEligibility(
            context.TargetPatient.GetAge(args.ScheduledDate),
            args.HasGuardianConsentVerified
        );

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            args.RequestedTimeSlot
        );

        return Appointment.Schedule(
            context.TargetPatient.Id,
            args.DoctorId,
            context.AppointmentType.Id,
            args.ScheduledDate,
            args.RequestedTimeSlot,
            context.InitiatorUserId
        );
    }
}

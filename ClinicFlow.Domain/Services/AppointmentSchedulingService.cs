using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Orchestrates appointment scheduling for different actors (Patient, Doctor, Staff).
/// </summary>
/// <remarks>
/// All scheduling requests must be accompanied by a valid scheduling clearance to ensure compliance with regional scheduling regulations.
/// </remarks>
public static class AppointmentSchedulingService
{
    public static Appointment ScheduleByPatient(
        AppointmentTypeDefinition appointmentType,
        PatientSchedulingArgs args,
        PatientSchedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointmentType);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.TargetPatient);
        ArgumentNullException.ThrowIfNull(args.TimeRange);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        PatientAccessService.VerifyAccess(context.InitiatorHasAccessToTarget);

        if (!args.IsInitiatorPhoneVerified)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.PhoneNotVerified
            );

        args.TargetPatient.EnsureCompleteProfile();

        new PenaltyHistory(context.Penalties).EnsureNotBlocked(args.ScheduledDate);

        bool isGuardianScheduling =
            context.InitiatorHasOwnSelfMembership && !context.TargetHasOwnSelfMembership;

        appointmentType.ValidatePatientEligibility(
            args.TargetPatient.GetAge(args.ScheduledDate),
            isGuardianScheduling
        );

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            args.TimeRange
        );

        return Appointment.Schedule(
            args.TargetPatient.Id,
            args.DoctorId,
            appointmentType.Id,
            args.ScheduledDate,
            args.TimeRange,
            args.PatientNotes
        );
    }

    public static Appointment ScheduleByDoctor(
        AppointmentTypeDefinition appointmentType,
        DoctorSchedulingArgs args,
        Schedule doctorSchedule,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointmentType);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.InitiatorDoctor);
        ArgumentNullException.ThrowIfNull(args.TargetPatient);
        ArgumentNullException.ThrowIfNull(args.TimeRange);
        ArgumentNullException.ThrowIfNull(doctorSchedule);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        appointmentType.ValidatePatientEligibility(
            args.TargetPatient.GetAge(args.ScheduledDate),
            args.HasGuardianConsentVerified
        );

        if (!args.IsOverbook)
            doctorSchedule.EnsureDoctorIsAvailable(
                args.InitiatorDoctor.Id,
                args.ScheduledDate.DayOfWeek,
                args.TimeRange
            );

        return Appointment.Schedule(
            args.TargetPatient.Id,
            args.InitiatorDoctor.Id,
            appointmentType.Id,
            args.ScheduledDate,
            args.TimeRange
        );
    }

    public static Appointment ScheduleByStaff(
        AppointmentTypeDefinition appointmentType,
        StaffSchedulingArgs args,
        Schedule doctorSchedule,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointmentType);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.TargetPatient);
        ArgumentNullException.ThrowIfNull(args.TimeRange);
        ArgumentNullException.ThrowIfNull(doctorSchedule);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        args.TargetPatient.EnsureCompleteProfile();

        appointmentType.ValidatePatientEligibility(
            args.TargetPatient.GetAge(args.ScheduledDate),
            args.HasGuardianConsentVerified
        );

        if (!args.IsOverbook)
            doctorSchedule.EnsureDoctorIsAvailable(
                args.DoctorId,
                args.ScheduledDate.DayOfWeek,
                args.TimeRange
            );

        return Appointment.Schedule(
            args.TargetPatient.Id,
            args.DoctorId,
            appointmentType.Id,
            args.ScheduledDate,
            args.TimeRange
        );
    }
}

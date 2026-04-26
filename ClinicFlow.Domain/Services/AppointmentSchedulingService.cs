using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service that orchestrates appointment scheduling,
/// enforcing actor-specific rules and constraints.
/// </summary>
/// <remarks>
/// Overbooking requests (bypassing availability and conflict checks) are permitted
/// exclusively for Doctor and Staff roles.
/// </remarks>
public static class AppointmentSchedulingService
{
    /// <summary>
    /// Schedules a new appointment on behalf of a patient, strictly enforcing ownership authorization,
    /// profile completeness, penalty blocks, schedule conflicts, eligibility rules, and doctor availability
    /// </summary>
    public static Appointment ScheduleByPatient(
        AppointmentTypeDefinition appointmentType,
        PatientSchedulingArgs args,
        AppointmentSchedulingContext context
    )
    {
        if (appointmentType is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (
            args is null
            || args.TargetPatient is null
            || args.InitiatorPatient is null
            || args.TimeRange is null
        )
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (args.TargetPatient.UserId != args.InitiatorPatient.UserId)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedScheduling
            );

        if (
            args.InitiatorPatient.RelationshipToUser is not PatientRelationship.Self
            && args.InitiatorPatient.Id != args.TargetPatient.Id
        )
        {
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedScheduling
            );
        }

        args.TargetPatient.EnsureCompleteProfile();
        Patient.EnsureNotBlocked(context.Penalties, args.ScheduledDate);

        bool isGuardianScheduling =
            args.InitiatorPatient.RelationshipToUser is PatientRelationship.Self
            && args.TargetPatient.RelationshipToUser is not PatientRelationship.Self;

        appointmentType.ValidatePatientEligibility(
            args.TargetPatient.GetAge(args.ScheduledDate),
            isGuardianScheduling
        );

        EnsureDoctorIsAvailable(
            context.DoctorSchedule,
            args.DoctorId,
            args.ScheduledDate,
            args.TimeRange
        );

        if (context.HasConflict)
            throw new AppointmentConflictException(
                DomainErrors.Appointment.Conflict,
                args.DoctorId,
                args.ScheduledDate.Add(args.TimeRange.Start)
            );

        return Appointment.Schedule(
            args.TargetPatient.Id,
            args.DoctorId,
            appointmentType.Id,
            args.ScheduledDate,
            args.TimeRange
        );
    }

    /// <summary>
    /// Schedules a new appointment on behalf of a doctor, restricted to Follow-up
    /// and Procedure categories.
    /// </summary>
    public static Appointment ScheduleByDoctor(
        AppointmentTypeDefinition appointmentType,
        DoctorSchedulingArgs args,
        AppointmentSchedulingContext context
    )
    {
        if (appointmentType is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (
            args is null
            || args.InitiatorDoctor is null
            || args.TargetPatient is null
            || args.TimeRange is null
        )
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        var allowedCategoriesForDoctors = new[]
        {
            AppointmentCategory.FollowUp,
            AppointmentCategory.Procedure,
        };

        if (!allowedCategoriesForDoctors.Contains(appointmentType.Category))
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedScheduling
            );

        if (!args.IsOverbook)
        {
            EnsureDoctorIsAvailable(
                context.DoctorSchedule,
                args.InitiatorDoctor.Id,
                args.ScheduledDate,
                args.TimeRange
            );

            if (context.HasConflict)
                throw new AppointmentConflictException(
                    DomainErrors.Appointment.Conflict,
                    args.InitiatorDoctor.Id,
                    args.ScheduledDate.Add(args.TimeRange.Start)
                );
        }

        return Appointment.Schedule(
            args.TargetPatient.Id,
            args.InitiatorDoctor.Id,
            appointmentType.Id,
            args.ScheduledDate,
            args.TimeRange
        );
    }

    /// <summary>
    /// Schedules a new appointment on behalf of a staff member, enforcing profile
    /// completeness and patient eligibility rules.
    /// </summary>
    public static Appointment ScheduleByStaff(
        AppointmentTypeDefinition appointmentType,
        StaffSchedulingArgs args,
        AppointmentSchedulingContext context
    )
    {
        if (appointmentType is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (args is null || args.TargetPatient is null || args.TimeRange is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        args.TargetPatient.EnsureCompleteProfile();

        appointmentType.ValidatePatientEligibility(
            args.TargetPatient.GetAge(args.ScheduledDate),
            args.HasGuardianConsentVerified
        );

        if (!args.IsOverbook)
        {
            EnsureDoctorIsAvailable(
                context.DoctorSchedule,
                args.DoctorId,
                args.ScheduledDate,
                args.TimeRange
            );

            if (context.HasConflict)
                throw new AppointmentConflictException(
                    DomainErrors.Appointment.Conflict,
                    args.DoctorId,
                    args.ScheduledDate.Add(args.TimeRange.Start)
                );
        }

        return Appointment.Schedule(
            args.TargetPatient.Id,
            args.DoctorId,
            appointmentType.Id,
            args.ScheduledDate,
            args.TimeRange
        );
    }

    private static void EnsureDoctorIsAvailable(
        Schedule? schedule,
        Guid doctorId,
        DateTime scheduledDate,
        TimeRange timeRange
    )
    {
        if (schedule is null || !schedule.CoversTimeRange(timeRange))
            throw new DoctorNotAvailableException(
                DomainErrors.Schedule.DoctorNotAvailable,
                doctorId,
                scheduledDate.DayOfWeek
            );
    }
}

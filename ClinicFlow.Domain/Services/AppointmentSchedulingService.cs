using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service that orchestrates appointment scheduling and rescheduling,
/// enforcing availability and conflict rules.
/// </summary>
public static class AppointmentSchedulingService
{
    /// <summary>
    /// Schedules a new appointment on behalf of a patient, enforcing strict domain invariants like account ownership and guardian consent.
    /// </summary>
    public static Appointment ScheduleByPatient(
        AppointmentTypeDefinition appointmentType,
        PatientSchedulingArgs args,
        AppointmentSchedulingContext context
    )
    {
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
        Patient.EnsureNotBlocked(context.Penalties);

        bool isGuardianScheduling =
            args.InitiatorPatient.RelationshipToUser is PatientRelationship.Self
            && args.TargetPatient.RelationshipToUser is not PatientRelationship.Self;

        appointmentType.ValidatePatientEligibility(
            args.TargetPatient.GetAge(),
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
    /// Schedules a new appointment on behalf of a doctor. Used for Follow-ups or Procedures.
    /// </summary>
    public static Appointment ScheduleByDoctor(
        AppointmentTypeDefinition appointmentType,
        DoctorSchedulingArgs args,
        AppointmentSchedulingContext context
    )
    {
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
    /// Schedules a new appointment on behalf of a staff member (receptionist).
    /// </summary>
    public static Appointment ScheduleByStaff(
        AppointmentTypeDefinition appointmentType,
        StaffSchedulingArgs args,
        AppointmentSchedulingContext context
    )
    {
        args.TargetPatient.EnsureCompleteProfile();

        appointmentType.ValidatePatientEligibility(
            args.TargetPatient.GetAge(),
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

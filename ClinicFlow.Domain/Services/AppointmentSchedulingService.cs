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
    /// Schedules a new appointment on behalf of a patient, enforcing ownership
    /// authorization, profile completeness, eligibility rules, and doctor availability.
    /// </summary>
    /// <param name="appointmentType">The definition of the appointment type, used to validate patient eligibility.</param>
    /// <param name="args">The scheduling arguments containing the initiator and target patients, the selected doctor, and the desired date and time.</param>
    /// <param name="context">Contextual scheduling data, such as doctor's availability schedule and existing conflicts.</param>
    /// <returns>A successfully scheduled <see cref="Appointment"/> instance.</returns>
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
    /// and Procedure categories, and bypassing availability and conflict checks
    /// when overbooking is requested.
    /// </summary>
    /// <param name="appointmentType">The definition of the appointment type. Must be a Follow-up or Procedure.</param>
    /// <param name="args">The scheduling arguments containing the initiator doctor, the target patient, the desired date/time, and whether overbooking is requested.</param>
    /// <param name="context">Contextual scheduling data, such as the doctor's schedule and conflicts.</param>
    /// <returns>A successfully scheduled <see cref="Appointment"/> instance.</returns>
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
    /// Schedules a new appointment on behalf of a staff member, enforcing profile
    /// completeness and patient eligibility rules, and bypassing availability and
    /// conflict checks when overbooking is requested.
    /// </summary>
    /// <param name="appointmentType">The definition of the appointment type.</param>
    /// <param name="args">The scheduling arguments including the target patient, the doctor, the desired date/time, and flags for guardian consent and overbooking.</param>
    /// <param name="context">Contextual scheduling data, such as the doctor's schedule and possible conflicts.</param>
    /// <returns>A successfully scheduled <see cref="Appointment"/> instance.</returns>
    public static Appointment ScheduleByStaff(
        AppointmentTypeDefinition appointmentType,
        StaffSchedulingArgs args,
        AppointmentSchedulingContext context
    )
    {
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

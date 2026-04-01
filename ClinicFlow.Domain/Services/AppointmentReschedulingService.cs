using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service that orchestrates appointment rescheduling,
/// enforcing availability and conflict rules for different actors.
/// </summary>
public static class AppointmentReschedulingService
{
    /// <summary>
    /// Reschedules an existing appointment on behalf of a patient, enforcing strict domain invariants like account ownership.
    /// </summary>
    public static void RescheduleByPatient(
        Appointment appointment,
        PatientReschedulingArgs args,
        AppointmentReschedulingContext context
    )
    {
        if (args.TargetPatient.Id != appointment.PatientId)
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

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

        Patient.EnsureNotBlocked(context.Penalties, args.NewDate);

        EnsureDoctorIsAvailable(
            context.DoctorSchedule,
            appointment.DoctorId,
            args.NewDate,
            args.NewTimeRange
        );

        if (context.HasConflict)
            throw new AppointmentConflictException(
                DomainErrors.Appointment.Conflict,
                appointment.DoctorId,
                args.NewDate.Add(args.NewTimeRange.Start)
            );

        appointment.Reschedule(args.NewDate, args.NewTimeRange);
    }

    /// <summary>
    /// Reschedules an existing appointment on behalf of a doctor, handling overbooking if needed.
    /// </summary>
    public static void RescheduleByDoctor(
        Appointment appointment,
        DoctorReschedulingArgs args,
        AppointmentReschedulingContext context
    )
    {
        if (args.InitiatorDoctor.Id != appointment.DoctorId)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedScheduling
            );

        if (!args.IsOverbook)
        {
            EnsureDoctorIsAvailable(
                context.DoctorSchedule,
                appointment.DoctorId,
                args.NewDate,
                args.NewTimeRange
            );

            if (context.HasConflict)
                throw new AppointmentConflictException(
                    DomainErrors.Appointment.Conflict,
                    appointment.DoctorId,
                    args.NewDate.Add(args.NewTimeRange.Start)
                );
        }

        appointment.Reschedule(args.NewDate, args.NewTimeRange);
    }

    /// <summary>
    /// Reschedules an existing appointment on behalf of a staff member (receptionist).
    /// </summary>
    public static void RescheduleByStaff(
        Appointment appointment,
        StaffReschedulingArgs args,
        AppointmentReschedulingContext context
    )
    {
        if (!args.IsOverbook)
        {
            EnsureDoctorIsAvailable(
                context.DoctorSchedule,
                appointment.DoctorId,
                args.NewDate,
                args.NewTimeRange
            );

            if (context.HasConflict)
                throw new AppointmentConflictException(
                    DomainErrors.Appointment.Conflict,
                    appointment.DoctorId,
                    args.NewDate.Add(args.NewTimeRange.Start)
                );
        }

        appointment.Reschedule(args.NewDate, args.NewTimeRange);
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

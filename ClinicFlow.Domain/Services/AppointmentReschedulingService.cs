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
/// enforcing regional scheduling regulations, availability, and conflict rules for different actors.
/// </summary>
/// <remarks>
/// All rescheduling requests must be accompanied by a valid scheduling clearance to ensure compliance
/// with regional policies. Overbooking requests (bypassing availability and conflict checks) are strictly
/// restricted to Doctor and Staff roles. Patient-initiated rescheduling must always
/// pass availability checks and penalty validations.
/// </remarks>
public static class AppointmentReschedulingService
{
    /// <summary>
    /// Reschedules an existing appointment on behalf of a patient, enforcing ownership authorization, penalty rules, and doctor availability.
    /// </summary>
    public static void RescheduleByPatient(
        Appointment appointment,
        PatientReschedulingArgs args,
        AppointmentReschedulingContext context,
        SchedulingClearance clearance
    )
    {
        if (appointment is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (
            args is null
            || args.TargetPatient is null
            || args.InitiatorPatient is null
            || args.NewTimeRange is null
        )
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (clearance is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

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

        if (!args.IsInitiatorPhoneVerified)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.PhoneNotVerified
            );

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

    public static void RescheduleByDoctor(
        Appointment appointment,
        DoctorReschedulingArgs args,
        AppointmentReschedulingContext context,
        SchedulingClearance clearance
    )
    {
        if (appointment is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (args is null || args.InitiatorDoctor is null || args.NewTimeRange is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (clearance is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

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

    public static void RescheduleByStaff(
        Appointment appointment,
        StaffReschedulingArgs args,
        AppointmentReschedulingContext context,
        SchedulingClearance clearance
    )
    {
        if (appointment is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (args is null || args.NewTimeRange is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (clearance is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

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

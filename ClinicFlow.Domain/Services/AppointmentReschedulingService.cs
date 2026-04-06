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
    /// Reschedules an existing appointment on behalf of a patient, enforcing ownership authorization, penalty rules, and doctor availability.
    /// </summary>
    /// <param name="appointment">The existing appointment to be rescheduled.</param>
    /// <param name="args">The rescheduling arguments containing the initiator and target patients, and the proposed new date and time.</param>
    /// <param name="context">Contextual rescheduling data, such as the doctor's schedule, penalties, and possible conflicts.</param>
    /// <exception cref="DomainValidationException">Thrown when the target patient does not match the appointment's patient.</exception>
    /// <exception cref="AppointmentSchedulingUnauthorizedException">
    /// Thrown when the initiator does not own the target patient's account,
    /// or when a non-self patient attempts to reschedule for a different patient.
    /// </exception>
    /// <exception cref="PatientBlockedException">Thrown when the target patient is currently blocked from scheduling due to penalties.</exception>
    /// <exception cref="DoctorNotAvailableException">Thrown when the doctor's schedule does not cover the new time range.</exception>
    /// <exception cref="AppointmentConflictException">Thrown when the doctor has an overlapping appointment at the new time.</exception>
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
    /// Reschedules an existing appointment on behalf of a doctor, enforcing ownership
    /// authorization, and bypassing availability and conflict checks when overbooking
    /// is requested.
    /// </summary>
    /// <param name="appointment">The existing appointment to be rescheduled.</param>
    /// <param name="args">The rescheduling arguments containing the initiator doctor, the new date and time, and whether overbooking is requested.</param>
    /// <param name="context">Contextual rescheduling data, such as the doctor's schedule and conflicts.</param>
    /// <exception cref="AppointmentSchedulingUnauthorizedException">Thrown when the initiating doctor is not the doctor associated with the appointment.</exception>
    /// <exception cref="DoctorNotAvailableException">Thrown when the doctor is not available at the new time and overbooking is not requested.</exception>
    /// <exception cref="AppointmentConflictException">Thrown when there is a scheduling conflict at the new time and overbooking is not requested.</exception>
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
    /// Reprograma una cita existente en nombre de un miembro del personal, omitiendo
    /// las validaciones de disponibilidad y conflictos cuando se solicita sobrecupo.
    /// </summary>
    /// <param name="appointment">The existing appointment to be rescheduled.</param>
    /// <param name="args">The rescheduling arguments containing the new date and time, and whether overbooking is requested.</param>
    /// <param name="context">Contextual rescheduling data, such as the doctor's schedule and conflicts.</param>
    /// <exception cref="DoctorNotAvailableException">Thrown when the doctor is not available at the new time and overbooking is not requested.</exception>
    /// <exception cref="AppointmentConflictException">Thrown when there is a scheduling conflict at the new time and overbooking is not requested.</exception>
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

using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Services.Contexts;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service that orchestrates appointment scheduling and rescheduling,
/// enforcing availability and conflict rules.
/// </summary>
public static class AppointmentSchedulingService
{
    /// <summary>
    /// Schedules a new appointment after verifying patient eligibility, doctor availability, and absence of time conflicts.
    /// </summary>
    /// <param name="details">The details of the appointment to be scheduled.</param>
    /// <param name="context">The context containing necessary data like the doctor's schedule, penalties, and a flag indicating if there's a conflict.</param>
    /// <returns>The newly created <see cref="Appointment"/>.</returns>
    /// <exception cref="PatientBlockedException">Thrown when the patient is blocked from booking.</exception>
    /// <exception cref="DoctorNotAvailableException">Thrown when the doctor has no schedule covering the requested time.</exception>
    /// <exception cref="AppointmentConflictException">Thrown when the time slot is already occupied.</exception>
    public static Appointment ScheduleAppointment(AppointmentSchedulingDetails details, AppointmentSchedulingContext context)
    {
        Patient.EnsureNotBlocked(context.Penalties);

        EnsureDoctorIsAvailable(context.DoctorSchedule, details.DoctorId, details.ScheduledDate, details.TimeRange);

        if (context.HasConflict) throw new AppointmentConflictException(DomainErrors.Appointment.Conflict, details.DoctorId, details.ScheduledDate.Add(details.TimeRange.Start));

        return Appointment.Schedule(details.PatientId, details.DoctorId, details.AppointmentTypeId, details.ScheduledDate, details.TimeRange);
    }

    /// <summary>
    /// Reschedules an existing appointment to a new date and time after verifying patient eligibility, doctor availability, and absence of conflicts.
    /// </summary>
    /// <param name="appointment">The existing appointment to be rescheduled.</param>
    /// <param name="newDate">The new date for the appointment.</param>
    /// <param name="newTimeRange">The new time range for the appointment.</param>
    /// <param name="context">The context containing the doctor's schedule, penalties, and existing appointments for conflict detection.</param>
    /// <exception cref="DoctorNotAvailableException">Thrown when the doctor has no schedule covering the requested time.</exception>
    /// <exception cref="AppointmentConflictException">Thrown when the new time slot conflicts with another appointment.</exception>
    public static void RescheduleAppointment(Appointment appointment, DateTime newDate, TimeRange newTimeRange, AppointmentSchedulingContext context)
    {
        EnsureDoctorIsAvailable(context.DoctorSchedule, appointment.DoctorId, newDate, newTimeRange);

        if (context.ExistingAppointmentsDay.Any(a => a.Id != appointment.Id && a.Status is not AppointmentStatus.Cancelled && a.TimeRange.OverlapsWith(newTimeRange)))
            throw new AppointmentConflictException(DomainErrors.Appointment.Conflict, appointment.DoctorId, newDate.Add(newTimeRange.Start));

        appointment.Reschedule(newDate, newTimeRange);
    }

    // Helper
    private static void EnsureDoctorIsAvailable(Schedule? schedule, Guid doctorId, DateTime scheduledDate, TimeRange timeRange)
    {
        if (schedule is null || !schedule.CoversTimeRange(timeRange))
            throw new DoctorNotAvailableException(DomainErrors.Schedule.DoctorNotAvailable, doctorId, scheduledDate.DayOfWeek);
    }
}
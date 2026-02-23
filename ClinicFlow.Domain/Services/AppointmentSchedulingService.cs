using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Domain.Enums;

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
    /// <param name="hasConflict">Pre-computed flag indicating whether a scheduling conflict exists.</param>
    /// <returns>The newly created <see cref="Appointment"/>.</returns>
    /// <exception cref="Exceptions.Patients.PatientBlockedException">Thrown when the patient is blocked from booking.</exception>
    /// <exception cref="DoctorNotAvailableException">Thrown when the doctor has no schedule covering the requested time.</exception>
    /// <exception cref="AppointmentConflictException">Thrown when the time slot is already occupied.</exception>
    public static Appointment ScheduleAppointment(Patient patient, IEnumerable<PatientPenalty> penalties, Doctor doctor, DateTime scheduledDate,
        TimeRange timeRange, Guid appointmentTypeId, Schedule? doctorSchedule, bool hasConflict)
    {
        Patient.EnsureNotBlocked(penalties);

        EnsureDoctorIsAvailable(doctorSchedule, doctor.Id, scheduledDate, timeRange);

        if (hasConflict) throw new AppointmentConflictException(doctor.Id, scheduledDate.Add(timeRange.Start));

        return Appointment.Schedule(patient.Id, doctor.Id, appointmentTypeId, scheduledDate, timeRange);
    }

    /// <summary>
    /// Reschedules an existing appointment to a new date and time after verifying doctor availability and absence of conflicts.
    /// </summary>
    /// <param name="existingAppointmentsDay">All non-cancelled appointments for the doctor on the new date, used for conflict detection.</param>
    /// <exception cref="DoctorNotAvailableException">Thrown when the doctor has no schedule covering the requested time.</exception>
    /// <exception cref="AppointmentConflictException">Thrown when the new time slot conflicts with another appointment.</exception>
    public static void RescheduleAppointment(Appointment appointment, DateTime newDate, TimeRange newTimeRange, Schedule? doctorSchedule, IEnumerable<Appointment> existingAppointmentsDay)
    {
        EnsureDoctorIsAvailable(doctorSchedule, appointment.DoctorId, newDate, newTimeRange);

        if (existingAppointmentsDay.Any(a => a.Id != appointment.Id && a.Status is not AppointmentStatus.Cancelled && a.TimeRange.OverlapsWith(newTimeRange)))
            throw new AppointmentConflictException(appointment.DoctorId, newDate.Add(newTimeRange.Start));

        appointment.Reschedule(newDate, newTimeRange);
    }

    // Helper
    private static void EnsureDoctorIsAvailable(Schedule? schedule, Guid doctorId, DateTime scheduledDate, TimeRange timeRange)
    {
        if (schedule is null || !schedule.CoversTimeRange(timeRange)) throw new DoctorNotAvailableException(doctorId, scheduledDate.DayOfWeek);
    }
}
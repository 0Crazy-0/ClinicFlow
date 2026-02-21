using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services;

public class AppointmentSchedulingService
{
    public Appointment ScheduleAppointment(Patient patient, IEnumerable<PatientPenalty> penalties, Doctor doctor, DateTime scheduledDate,
        TimeRange timeRange, Guid appointmentTypeId, Schedule? doctorSchedule, bool hasConflict)
    {
        Patient.EnsureNotBlocked(penalties);

        EnsureDoctorIsAvailable(doctorSchedule, doctor.Id, scheduledDate, timeRange);

        if (hasConflict) throw new AppointmentConflictException(doctor.Id, scheduledDate.Add(timeRange.Start));

        return Appointment.Schedule(patient.Id, doctor.Id, appointmentTypeId, scheduledDate, timeRange);
    }

    public void RescheduleAppointment(Appointment appointment, DateTime newDate, TimeRange newTimeRange, Schedule? doctorSchedule, IEnumerable<Appointment> existingAppointmentsDay)
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
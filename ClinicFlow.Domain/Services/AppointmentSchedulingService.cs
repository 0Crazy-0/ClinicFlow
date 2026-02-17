using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services;

public class AppointmentSchedulingService(IAppointmentRepository appointmentRepository, IScheduleRepository scheduleRepository)
{
    public async Task<Appointment> ScheduleAppointmentAsync(Patient patient, IEnumerable<PatientPenalty> penalties, Doctor doctor, DateTime scheduledDate,
        TimeRange timeRange, Guid appointmentTypeId)
    {
        Patient.EnsureNotBlocked(penalties);

        await EnsureDoctorIsAvailableAsync(doctor.Id, scheduledDate, timeRange);

        if (await appointmentRepository.HasConflictAsync(doctor.Id, scheduledDate, timeRange))
            throw new AppointmentConflictException(doctor.Id, scheduledDate.Add(timeRange.Start));

        return Appointment.Schedule(patient.Id, doctor.Id, appointmentTypeId, scheduledDate, timeRange);
    }

    public async Task RescheduleAppointmentAsync(Appointment appointment, DateTime newDate, TimeRange newTimeRange)
    {
        await EnsureDoctorIsAvailableAsync(appointment.DoctorId, newDate, newTimeRange);

        var existingAppointments = await appointmentRepository.GetByDoctorIdAsync(appointment.DoctorId, newDate.Date);

        if (existingAppointments.Any(a => a.Id != appointment.Id && a.Status is not AppointmentStatus.Cancelled && a.TimeRange.OverlapsWith(newTimeRange)))
            throw new AppointmentConflictException(appointment.DoctorId, newDate.Add(newTimeRange.Start));

        appointment.Reschedule(newDate, newTimeRange);
    }

    // Helper
    private async Task EnsureDoctorIsAvailableAsync(Guid doctorId, DateTime scheduledDate, TimeRange timeRange)
    {
        var schedule = await scheduleRepository.GetByDoctorAndDayAsync(doctorId, scheduledDate.DayOfWeek);

        if (schedule is null || !schedule.CoversTimeRange(timeRange)) throw new DoctorNotAvailableException(doctorId, scheduledDate.DayOfWeek);
    }
}
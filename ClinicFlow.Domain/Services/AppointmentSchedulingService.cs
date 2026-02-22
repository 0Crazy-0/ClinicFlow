using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services;

public class AppointmentSchedulingService(IAppointmentRepository appointmentRepository, IScheduleRepository scheduleRepository, IPatientPenaltyRepository penaltyRepository)
{
    public async Task<Appointment> ScheduleAppointmentAsync(Guid patientId, Guid doctorId, DateTime scheduledDate, TimeRange timeRange, Guid appointmentTypeId)
    {
        var penalties = await penaltyRepository.GetByPatientIdAsync(patientId);

        Patient.EnsureNotBlocked(penalties);

        await EnsureDoctorIsAvailableAsync(doctorId, scheduledDate, timeRange);

        if (await appointmentRepository.HasConflictAsync(doctorId, scheduledDate, timeRange)) throw new AppointmentConflictException(doctorId, scheduledDate.Add(timeRange.Start));

        return Appointment.Schedule(patientId, doctorId, appointmentTypeId, scheduledDate, timeRange);
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
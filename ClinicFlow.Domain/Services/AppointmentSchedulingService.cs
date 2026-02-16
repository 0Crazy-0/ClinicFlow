using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services;

public class AppointmentSchedulingService(IAppointmentRepository appointmentRepository)
{
    public async Task<Appointment> ScheduleAppointmentAsync(Patient patient, IEnumerable<PatientPenalty> penalties, Doctor doctor, DateTime scheduledDate,
        TimeRange timeRange, Guid appointmentTypeId)
    {
        Patient.EnsureNotBlocked(penalties);

        if (await appointmentRepository.HasConflictAsync(doctor.Id, scheduledDate, timeRange)) 
            throw new AppointmentConflictException(doctor.Id, scheduledDate.Add(timeRange.Start));

        return Appointment.Schedule(patient.Id, doctor.Id, appointmentTypeId, scheduledDate, timeRange);
    }

    public async Task RescheduleAppointmentAsync(Appointment appointment, DateTime newDate, TimeRange newTimeRange)
    {
        var existingAppointments = await appointmentRepository.GetByDoctorIdAsync(appointment.DoctorId, newDate.Date);

        if (existingAppointments.Any(a => a.Id != appointment.Id && a.Status is not AppointmentStatus.Cancelled && a.TimeRange.OverlapsWith(newTimeRange)))
            throw new AppointmentConflictException(appointment.DoctorId, newDate.Add(newTimeRange.Start));

        appointment.Reschedule(newDate, newTimeRange);
    }
}
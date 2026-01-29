using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Domain.Interfaces;

namespace ClinicFlow.Domain.Services;
public class AppointmentSchedulingService(IAppointmentRepository appointmentRepository)
{
    public async Task<Appointment> ScheduleAppointmentAsync(Patient patient, IEnumerable<PatientPenalty> penalties, Doctor doctor, DateTime scheduledDate, TimeRange timeRange, Guid appointmentTypeId)
    {
        patient.EnsureNotBlocked(penalties);

        if (await appointmentRepository.HasConflictAsync(doctor.Id, scheduledDate, timeRange.Start, timeRange.End))
            throw new AppointmentConflictException(doctor.Id, scheduledDate.Add(timeRange.Start));

        return Appointment.Schedule(patient.Id, doctor.Id, appointmentTypeId, scheduledDate, timeRange);
    }
}
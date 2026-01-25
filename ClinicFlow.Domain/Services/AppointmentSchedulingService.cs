using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Domain.Interfaces;

namespace ClinicFlow.Domain.Services;
public class AppointmentSchedulingService(IAppointmentRepository appointmentRepository)
{
    public async Task<Appointment> ScheduleAppointmentAsync(Patient patient, Doctor doctor, DateTime scheduledDate, TimeRange timeRange, Guid appointmentTypeId)
    {
        if (patient.IsBlockedFromBooking())
        {
            var blockUntil = patient.Penalties.Where(p => p.PenaltyType is PenaltyType.TemporaryBlock && p.BlockedUntil > DateTime.UtcNow)
                .Max(p => p.BlockedUntil) ?? DateTime.UtcNow;

            throw new PatientBlockedException(blockUntil);
        }

        if (await appointmentRepository.HasConflictAsync(doctor.Id, scheduledDate, timeRange.Start, timeRange.End))
            throw new AppointmentConflictException(doctor.Id, scheduledDate.Add(timeRange.Start));

        return Appointment.Schedule(patient.Id, doctor.Id, appointmentTypeId, scheduledDate, timeRange);
    }
}
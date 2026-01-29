using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<List<Appointment>> GetByDoctorIdAsync(Guid doctorId, DateTime date);
    Task<List<Appointment>> GetByPatientIdAsync(Guid patientId);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(Guid id);
    Task<bool> HasConflictAsync(Guid doctorId, DateTime scheduledDate, TimeSpan startTime, TimeSpan endTime);
    Task<List<Appointment>> GetUpcomingByPatientAsync(Guid patientId);
    Task<List<Appointment>> GetByStatusAsync(AppointmentStatusEnum status);
}

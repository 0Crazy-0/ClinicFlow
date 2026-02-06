using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<IList<Appointment>> GetByDoctorIdAsync(Guid doctorId, DateTime date);
    Task<IList<Appointment>> GetByPatientIdAsync(Guid patientId);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(Guid id);
    Task<bool> HasConflictAsync(Guid doctorId, DateTime scheduledDate, TimeRange timeRange);
    Task<IList<Appointment>> GetUpcomingByPatientAsync(Guid patientId);
    Task<IList<Appointment>> GetByStatusAsync(AppointmentStatusEnum status);
}

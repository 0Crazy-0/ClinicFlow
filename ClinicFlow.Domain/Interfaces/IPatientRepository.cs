using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id);
    Task<Patient?> GetByUserIdAsync(Guid userId);
    Task<Patient> CreateAsync(Patient patient);
    Task UpdateAsync(Patient patient);
}

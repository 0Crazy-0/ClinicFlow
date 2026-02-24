using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Patient"/> persistence operations.
/// </summary>
public interface IPatientRepository
{
    /// <summary>
    /// Retrieves a patient by its unique identifier.
    /// </summary>
    Task<Patient?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves a patient by the associated user account identifier.
    /// </summary>
    Task<Patient?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Persists a new patient.
    /// </summary>
    Task<Patient> CreateAsync(Patient patient);

    /// <summary>
    /// Updates an existing patient.
    /// </summary>
    Task UpdateAsync(Patient patient);
}

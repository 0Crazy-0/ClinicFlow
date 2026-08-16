using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Patient"/> persistence operations.
/// </summary>
public interface IPatientRepository
{
    Task CreateAsync(Patient patient, CancellationToken cancellationToken = default);

    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Patient?> GetByNameAndDobAsync(
        PersonName fullName,
        DateOnly dateOfBirth,
        CancellationToken cancellationToken = default
    );
}

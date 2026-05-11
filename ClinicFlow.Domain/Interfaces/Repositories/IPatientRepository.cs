using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Patient"/> persistence operations.
/// </summary>
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Patient?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<Patient>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<Patient?> GetIncludingDeletedByNameAndDobAsync(
        Guid userId,
        PersonName fullName,
        DateTime dateOfBirth,
        CancellationToken ct = default
    );

    Task<Patient> CreateAsync(Patient patient, CancellationToken ct = default);
}

using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Patient"/> persistence operations.
/// </summary>
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Patient?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Patient>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<Patient?> GetIncludingDeletedByNameAndDobAsync(
        Guid userId,
        PersonName fullName,
        DateTime dateOfBirth,
        CancellationToken cancellationToken = default
    );

    Task<Patient> CreateAsync(Patient patient, CancellationToken cancellationToken = default);
}

using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Patient"/> persistence operations.
/// </summary>
public interface IPatientRepository
{
    Task CreateAsync(Patient patient, CancellationToken cancellationToken = default);

    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithSsnLast4HashAsync(
        Guid patientId,
        string ssnLast4Hash,
        CancellationToken cancellationToken = default
    );
}

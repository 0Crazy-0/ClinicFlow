using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="PatientActivationCode"/> persistence operations.
/// </summary>
public interface IPatientActivationCodeRepository
{
    Task CreateAsync(
        PatientActivationCode activationCode,
        CancellationToken cancellationToken = default
    );

    /// <remarks>
    /// A code is active while it is neither consumed nor superseded. No ordering is applied because
    /// the filtered unique index on <c>UserId</c> (<c>"ConsumedAt" IS NULL AND "SupersededAt" IS NULL</c>)
    /// guarantees at most one active code per user, so the query can never match more than one row.
    /// </remarks>
    Task<PatientActivationCode?> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}

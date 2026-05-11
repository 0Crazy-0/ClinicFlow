using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="PatientPenalty"/> persistence operations.
/// </summary>
public interface IPatientPenaltyRepository
{
    Task<PatientPenalty?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(PatientPenalty penalty, CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<PatientPenalty> penalties, CancellationToken ct = default);

    Task<IReadOnlyList<PatientPenalty>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<PatientPenalty>> GetActiveBlocksAsync(
        DateTime referenceTime,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<PatientPenalty>> GetActiveWarningsAsync(CancellationToken ct = default);
}

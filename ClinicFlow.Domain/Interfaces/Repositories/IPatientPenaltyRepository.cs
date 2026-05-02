using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="PatientPenalty"/> persistence operations.
/// </summary>
public interface IPatientPenaltyRepository
{
    Task<PatientPenalty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(PatientPenalty penalty, CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<PatientPenalty> penalties,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<PatientPenalty>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<PatientPenalty>> GetActiveBlocksAsync(
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<PatientPenalty>> GetActiveWarningsAsync(
        CancellationToken cancellationToken = default
    );
}

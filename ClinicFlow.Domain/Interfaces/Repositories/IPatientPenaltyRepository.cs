using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="PatientPenalty"/> persistence operations.
/// </summary>
public interface IPatientPenaltyRepository
{
    Task<PatientPenalty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PatientPenalty> CreateAsync(
        PatientPenalty penalty,
        CancellationToken cancellationToken = default
    );

    Task CreateRangeAsync(
        IEnumerable<PatientPenalty> penalties,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<PatientPenalty>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyList<PatientPenalty> Items, int TotalCount)> GetByPatientIdPaginatedAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyList<PatientPenalty> Items, int TotalCount)> GetActiveBlocksPaginatedAsync(
        DateTime referenceTime,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<PatientPenalty>> GetActiveWarningsAsync(
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyList<PatientPenalty> Items, int TotalCount)> GetActiveWarningsPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
}

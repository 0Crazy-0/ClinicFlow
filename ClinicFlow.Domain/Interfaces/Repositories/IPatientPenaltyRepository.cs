using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="PatientPenalty"/> persistence operations.
/// </summary>
public interface IPatientPenaltyRepository
{
    Task<PatientPenalty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(PatientPenalty penalty, CancellationToken cancellationToken = default);

    Task CreateRangeAsync(
        IEnumerable<PatientPenalty> penalties,
        CancellationToken cancellationToken = default
    );

    /// <remarks>
    /// Includes penalties regardless of their current status — this covers
    /// penalties marked as removed (<see cref="PatientPenalty.IsRemoved"/> is <c>true</c>)
    /// and temporary blocks whose <see cref="PatientPenalty.BlockedUntil"/> date has already passed.
    /// </remarks>
    Task<IReadOnlyList<PatientPenalty>> GetHistoryByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default
    );

    /// <inheritdoc cref="GetHistoryByPatientIdAsync"/>
    Task<(IReadOnlyList<PatientPenalty> Items, int TotalCount)> GetHistoryByPatientIdPaginatedAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyList<PatientPenalty> Items, int TotalCount)> GetActiveBlocksPaginatedAsync(
        DateOnly referenceDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyList<PatientPenalty> Items, int TotalCount)> GetActiveWarningsPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
}

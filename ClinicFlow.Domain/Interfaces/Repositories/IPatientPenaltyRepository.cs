using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="PatientPenalty"/> persistence operations.
/// </summary>
public interface IPatientPenaltyRepository
{
    Task AddRangeAsync(
        IEnumerable<PatientPenalty> penalties,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<PatientPenalty>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default
    );
}

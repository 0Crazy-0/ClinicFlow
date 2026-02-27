using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="PatientPenalty"/> persistence operations.
/// </summary>
public interface IPatientPenaltyRepository
{
    Task AddAsync(PatientPenalty penalty);

    Task<IEnumerable<PatientPenalty>> GetByPatientIdAsync(Guid patientId);
}

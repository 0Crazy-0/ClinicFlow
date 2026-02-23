using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces;

/// <summary>
/// Repository contract for <see cref="PatientPenalty"/> persistence operations.
/// </summary>
public interface IPatientPenaltyRepository
{
    /// <summary>
    /// Persists a new patient penalty.
    /// </summary>
    Task AddAsync(PatientPenalty penalty);

    /// <summary>
    /// Retrieves all penalties for a given patient.
    /// </summary>
    Task<IEnumerable<PatientPenalty>> GetByPatientIdAsync(Guid patientId);
}

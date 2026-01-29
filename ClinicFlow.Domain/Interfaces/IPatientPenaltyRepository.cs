using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces;

public interface IPatientPenaltyRepository
{
    Task AddAsync(PatientPenalty penalty);
    Task<IEnumerable<PatientPenalty>> GetByPatientIdAsync(Guid patientId);
}

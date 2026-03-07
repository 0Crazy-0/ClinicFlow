using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

public interface IClinicalFormTemplateRepository
{
    Task<ClinicalFormTemplate?> GetByCodeAsync(string templateCode, CancellationToken cancellationToken = default);
}

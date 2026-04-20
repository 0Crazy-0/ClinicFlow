using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

public interface IClinicalFormTemplateRepository
{
    Task<ClinicalFormTemplate?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<ClinicalFormTemplate?> GetByCodeAsync(
        string templateCode,
        CancellationToken cancellationToken = default
    );

    Task<ClinicalFormTemplate> CreateAsync(
        ClinicalFormTemplate template,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(ClinicalFormTemplate template, CancellationToken cancellationToken = default);
}

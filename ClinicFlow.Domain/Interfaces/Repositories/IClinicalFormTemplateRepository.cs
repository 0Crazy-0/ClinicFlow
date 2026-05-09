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

    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<ClinicalFormTemplate?> GetByIdIncludingDeletedAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByNameExcludingAsync(
        string name,
        Guid excludeId,
        CancellationToken cancellationToken = default
    );

    Task<ClinicalFormTemplate> CreateAsync(
        ClinicalFormTemplate template,
        CancellationToken cancellationToken = default
    );
}

using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

public interface IClinicalFormTemplateRepository
{
    Task<ClinicalFormTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<ClinicalFormTemplate?> GetByCodeAsync(string templateCode, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

    Task<ClinicalFormTemplate?> GetByIdIncludingDeletedAsync(
        Guid id,
        CancellationToken ct = default
    );

    Task<bool> ExistsByNameExcludingAsync(
        string name,
        Guid excludeId,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<ClinicalFormTemplate>> GetAllActiveAsync(CancellationToken ct = default);

    Task<IReadOnlyList<ClinicalFormTemplate>> GetAllIncludingDeletedAsync(
        CancellationToken ct = default
    );

    Task<ClinicalFormTemplate> CreateAsync(
        ClinicalFormTemplate template,
        CancellationToken ct = default
    );
}

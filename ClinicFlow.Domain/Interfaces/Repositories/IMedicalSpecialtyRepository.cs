using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="MedicalSpecialty"/> persistence operations.
/// </summary>
public interface IMedicalSpecialtyRepository
{
    Task<MedicalSpecialty?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<MedicalSpecialty?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

    Task<bool> ExistsByNameExcludingAsync(
        string name,
        Guid excludeId,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<MedicalSpecialty>> GetAllActiveAsync(CancellationToken ct = default);

    Task<IReadOnlyList<MedicalSpecialty>> GetAllIncludingDeletedAsync(
        CancellationToken ct = default
    );

    Task<MedicalSpecialty> CreateAsync(MedicalSpecialty specialty, CancellationToken ct = default);
}

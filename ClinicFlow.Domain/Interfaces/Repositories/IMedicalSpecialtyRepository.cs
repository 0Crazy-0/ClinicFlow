using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="MedicalSpecialty"/> persistence operations.
/// </summary>
public interface IMedicalSpecialtyRepository
{
    Task CreateAsync(MedicalSpecialty specialty, CancellationToken cancellationToken = default);

    Task<MedicalSpecialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MedicalSpecialty?> GetByIdIncludingDeletedAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameExcludingAsync(
        string name,
        Guid excludeId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<MedicalSpecialty>> GetAllActiveAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<MedicalSpecialty>> GetAllIncludingDeletedAsync(
        CancellationToken cancellationToken = default
    );
}

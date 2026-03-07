using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="MedicalSpecialty"/> persistence operations.
/// </summary>
public interface IMedicalSpecialtyRepository
{
    Task<MedicalSpecialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

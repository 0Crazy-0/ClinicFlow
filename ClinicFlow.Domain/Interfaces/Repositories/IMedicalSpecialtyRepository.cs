using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="MedicalSpecialty"/> persistence operations.
/// </summary>
public interface IMedicalSpecialtyRepository
{
    /// <summary>
    /// Retrieves a medical specialty by its unique identifier.
    /// </summary>
    Task<MedicalSpecialty?> GetByIdAsync(Guid id);
}

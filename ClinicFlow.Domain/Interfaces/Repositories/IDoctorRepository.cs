using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Doctor"/> persistence operations.
/// </summary>
public interface IDoctorRepository
{
    /// <summary>
    /// Retrieves a doctor by its unique identifier.
    /// </summary>
    Task<Doctor?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves a doctor by the associated user account identifier.
    /// </summary>
    Task<Doctor?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Retrieves all doctors belonging to a specific medical specialty.
    /// </summary>
    Task<List<Doctor>> GetBySpecialtyIdAsync(Guid specialtyId);

    /// <summary>
    /// Retrieves all registered doctors.
    /// </summary>
    Task<List<Doctor>> GetAllAsync();

    /// <summary>
    /// Persists a new doctor.
    /// </summary>
    Task<Doctor> CreateAsync(Doctor doctor);

    /// <summary>
    /// Updates an existing doctor.
    /// </summary>
    Task UpdateAsync(Doctor doctor);

    /// <summary>
    /// Checks whether a doctor with the given license number already exists.
    /// </summary>
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber);
}

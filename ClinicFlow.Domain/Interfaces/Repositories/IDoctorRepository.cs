using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Doctor"/> persistence operations.
/// </summary>
public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Doctor?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<Doctor>> GetBySpecialtyIdAsync(
        Guid specialtyId,
        CancellationToken ct = default
    );

    Task<Doctor> CreateAsync(Doctor doctor, CancellationToken ct = default);

    Task<bool> HasActiveBySpecialtyIdAsync(Guid specialtyId, CancellationToken ct = default);

    Task<Doctor?> GetIncludingDeletedByLicenseNumberAsync(
        string licenseNumber,
        CancellationToken ct = default
    );
}

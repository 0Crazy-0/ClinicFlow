using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Doctor"/> persistence operations.
/// </summary>
public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Doctor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Doctor> Items, int TotalCount)> GetBySpecialtyIdPaginatedAsync(
        Guid specialtyId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task CreateAsync(Doctor doctor, CancellationToken cancellationToken = default);

    Task<bool> HasActiveBySpecialtyIdAsync(
        Guid specialtyId,
        CancellationToken cancellationToken = default
    );

    Task<Doctor?> GetIncludingDeletedByLicenseNumberAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default
    );
}

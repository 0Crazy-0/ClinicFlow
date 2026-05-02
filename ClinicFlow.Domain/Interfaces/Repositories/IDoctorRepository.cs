using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Doctor"/> persistence operations.
/// </summary>
public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Doctor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Doctor>> GetBySpecialtyIdAsync(
        Guid specialtyId,
        CancellationToken cancellationToken = default
    );

    Task<Doctor> CreateAsync(Doctor doctor, CancellationToken cancellationToken = default);
}

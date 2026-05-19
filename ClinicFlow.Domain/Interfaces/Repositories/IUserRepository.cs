using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="User"/> persistence operations.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyCollection<User> Items, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        UserRole? role,
        bool? isActive,
        string? searchTerm,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyCollection<User> Items, int TotalCount)> GetLockedOutUsersPaginatedAsync(
        DateTime referenceTime,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
}

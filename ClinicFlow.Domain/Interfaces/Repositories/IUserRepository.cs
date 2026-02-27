using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="User"/> persistence operations.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);

    Task<User?> GetByEmailAsync(string email);

    Task<User> CreateAsync(User user);

    Task UpdateAsync(User user);

    Task DeleteAsync(Guid id);

    Task<bool> ExistsByEmailAsync(string email);
}

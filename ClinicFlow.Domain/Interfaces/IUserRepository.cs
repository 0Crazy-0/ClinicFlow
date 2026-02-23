using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces;

/// <summary>
/// Repository contract for <see cref="User"/> persistence operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by its unique identifier.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Persists a new user.
    /// </summary>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    Task UpdateAsync(User user);

    /// <summary>
    /// Deletes a user by its identifier.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks whether a user with the given email address already exists.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);
}

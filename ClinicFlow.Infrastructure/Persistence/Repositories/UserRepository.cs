using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="User"/> persistence operations.
/// </summary>
public sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);
        return Task.CompletedTask;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        var emailAddress = EmailAddress.Create(email);

        return await dbContext.Users.FirstOrDefaultAsync(
            user => user.Email == emailAddress,
            cancellationToken
        );
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        var emailAddress = EmailAddress.Create(email);

        return await dbContext.Users.AnyAsync(
            user => user.Email == emailAddress,
            cancellationToken
        );
    }

    public async Task<bool> ExistsByPhoneNumberAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedPhoneNumber = PhoneNumber.Create(phoneNumber);

        return await dbContext.Users.AnyAsync(
            user => user.PhoneNumber == normalizedPhoneNumber,
            cancellationToken
        );
    }

    public async Task<(IReadOnlyCollection<User> Items, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        UserRole? role,
        bool? isActive,
        string? searchTerm,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.Users.AsNoTracking();

        if (role.HasValue)
            query = query.Where(user => user.Role == role.Value);

        if (isActive.HasValue)
            query = query.Where(user => user.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchPattern = $"%{searchTerm.Trim()}%";

            query = query.Where(user =>
                EF.Functions.ILike(user.Email, searchPattern)
                || EF.Functions.ILike(user.PhoneNumber, searchPattern)
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(user => user.SequenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(
        IReadOnlyCollection<User> Items,
        int TotalCount
    )> GetLockedOutUsersPaginatedAsync(
        DateTime referenceTime,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.Users.AsNoTracking().Where(user => user.LockoutEnd > referenceTime);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(user => user.SequenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="PatientActivationCode"/> persistence operations.
/// </summary>
public sealed class PatientActivationCodeRepository(ApplicationDbContext dbContext)
    : IPatientActivationCodeRepository
{
    public Task CreateAsync(
        PatientActivationCode activationCode,
        CancellationToken cancellationToken = default
    )
    {
        if (dbContext.Entry(activationCode).State is EntityState.Detached)
            dbContext.PatientActivationCodes.Add(activationCode);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<PatientActivationCode?> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .PatientActivationCodes.Where(code =>
                code.UserId == userId && code.ConsumedAt == null && code.SupersededAt == null
            )
            .OrderByDescending(code => code.IssuedAt)
            .FirstOrDefaultAsync(cancellationToken);
}

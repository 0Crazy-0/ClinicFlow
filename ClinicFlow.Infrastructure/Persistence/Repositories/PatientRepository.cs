using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="Patient"/> persistence operations.
/// </summary>
public sealed class PatientRepository(ApplicationDbContext dbContext) : IPatientRepository
{
    public async Task<Patient?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => await dbContext.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Patient?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    ) => await dbContext.Patients.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Patient>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    ) => await dbContext.Patients.Where(p => p.UserId == userId).ToListAsync(cancellationToken);

    public async Task<Patient?> GetIncludingDeletedByNameAndDobAsync(
        Guid userId,
        PersonName fullName,
        DateOnly dateOfBirth,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .Patients.IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                p => p.UserId == userId && p.FullName == fullName && p.DateOfBirth == dateOfBirth,
                cancellationToken
            );

    public Task CreateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        dbContext.Patients.Add(patient);
        return Task.CompletedTask;
    }
}

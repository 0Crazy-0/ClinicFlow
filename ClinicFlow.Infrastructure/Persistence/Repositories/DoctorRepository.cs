using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for <see cref="Doctor"/> persistence operations.
/// </summary>
public sealed class DoctorRepository(ApplicationDbContext dbContext) : IDoctorRepository
{
    public async Task<Doctor?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => await dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<Doctor?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    ) => await dbContext.Doctors.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

    public async Task<(IReadOnlyList<Doctor> Items, int TotalCount)> GetBySpecialtyIdPaginatedAsync(
        Guid specialtyId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .Doctors.AsNoTracking()
            .Where(d => d.MedicalSpecialtyId == specialtyId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(d => d.FullName)
            .ThenBy(d => d.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task CreateAsync(Doctor doctor, CancellationToken cancellationToken = default) =>
        await dbContext.Doctors.AddAsync(doctor, cancellationToken);

    public async Task<bool> HasActiveBySpecialtyIdAsync(
        Guid specialtyId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.Doctors.AnyAsync(
            d => d.MedicalSpecialtyId == specialtyId,
            cancellationToken
        );

    public async Task<Doctor?> GetIncludingDeletedByLicenseNumberAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedLicense = MedicalLicenseNumber.Create(licenseNumber);

        return await dbContext
            .Doctors.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.LicenseNumber == normalizedLicense, cancellationToken);
    }
}

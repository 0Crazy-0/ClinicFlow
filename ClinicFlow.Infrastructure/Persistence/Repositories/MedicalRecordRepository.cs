using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="MedicalRecord"/> persistence operations.
/// </summary>
public sealed class MedicalRecordRepository(ApplicationDbContext dbContext)
    : IMedicalRecordRepository
{
    public async Task<MedicalRecord?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .MedicalRecords.Include(m => m.ClinicalDetails)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<(
        IReadOnlyList<MedicalRecord> Items,
        int TotalCount
    )> GetByPatientIdPaginatedAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .MedicalRecords.Include(m => m.ClinicalDetails)
            .AsNoTracking()
            .Where(m => m.PatientId == patientId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(m => m.SequenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(
        IReadOnlyList<MedicalRecord> Items,
        int TotalCount
    )> GetByDoctorIdPaginatedAsync(
        Guid doctorId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .MedicalRecords.Include(m => m.ClinicalDetails)
            .AsNoTracking()
            .Where(m => m.DoctorId == doctorId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(m => m.SequenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<MedicalRecord?> GetByAppointmentIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .MedicalRecords.Include(m => m.ClinicalDetails)
            .FirstOrDefaultAsync(m => m.AppointmentId == appointmentId, cancellationToken);

    public async Task CreateAsync(
        MedicalRecord medicalRecord,
        CancellationToken cancellationToken = default
    ) => await dbContext.MedicalRecords.AddAsync(medicalRecord, cancellationToken);
}

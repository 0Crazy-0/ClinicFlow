using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="MedicalRecord"/> persistence operations.
/// </summary>
public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<MedicalRecord> Items, int TotalCount)> GetByPatientIdPaginatedAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyList<MedicalRecord> Items, int TotalCount)> GetByDoctorIdPaginatedAsync(
        Guid doctorId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<MedicalRecord?> GetByAppointmentIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default
    );

    Task<MedicalRecord> CreateAsync(
        MedicalRecord medicalRecord,
        CancellationToken cancellationToken = default
    );
}

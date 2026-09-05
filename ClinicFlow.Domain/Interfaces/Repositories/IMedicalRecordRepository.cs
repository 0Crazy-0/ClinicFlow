using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="MedicalRecord"/> persistence operations.
/// </summary>
public interface IMedicalRecordRepository
{
    Task CreateAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken = default);

    Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<MedicalRecord> Items, int TotalCount)> GetByPatientIdPaginatedAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the patient's medical records paginated, excluding the ones whose protected
    /// category is blocked for the requester. Records without a protected category are never
    /// excluded.
    /// </summary>
    Task<(
        IReadOnlyList<MedicalRecord> Items,
        int TotalCount
    )> GetByPatientIdPaginatedExcludingCategoriesAsync(
        Guid patientId,
        IReadOnlyCollection<ProtectedCategory> excludedCategories,
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
}

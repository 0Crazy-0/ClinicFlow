using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="MedicalRecordConsentGrant"/> persistence operations.
/// </summary>
public interface IMedicalRecordConsentGrantRepository
{
    Task CreateAsync(
        MedicalRecordConsentGrant grant,
        CancellationToken cancellationToken = default
    );

    Task<MedicalRecordConsentGrant?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasEffectiveGrantAsync(
        Guid medicalRecordId,
        Guid recipientMembershipId,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Guid>> GetEffectiveRecordIdsAsync(
        Guid patientId,
        Guid recipientMembershipId,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    );

    Task<(
        IReadOnlyList<MedicalRecordConsentGrant> Items,
        int TotalCount
    )> GetByPatientIdPaginatedAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );
}

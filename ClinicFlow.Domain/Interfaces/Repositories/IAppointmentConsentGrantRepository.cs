using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Interfaces.Repositories;

public interface IAppointmentConsentGrantRepository
{
    Task CreateAsync(AppointmentConsentGrant grant, CancellationToken cancellationToken = default);

    Task<AppointmentConsentGrant?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasEffectiveCategoryGrantAsync(
        Guid patientId,
        Guid recipientMembershipId,
        ProtectedCategory protectedCareCategory,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasEffectiveAppointmentGrantAsync(
        Guid patientId,
        Guid recipientMembershipId,
        Guid appointmentId,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ProtectedCategory>> GetEffectiveCategoriesAsync(
        Guid patientId,
        Guid recipientMembershipId,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Guid>> GetEffectiveAppointmentIdsAsync(
        Guid patientId,
        Guid recipientMembershipId,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    );
}

using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="AppointmentTypeDefinition"/> persistence operations.
/// </summary>
public interface IAppointmentTypeDefinitionRepository
{
    Task<AppointmentTypeDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<AppointmentTypeDefinition>> GetAllActiveAsync(
        CancellationToken ct = default
    );

    Task<IReadOnlyList<AppointmentTypeDefinition>> GetByCategoryAsync(
        AppointmentCategory category,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<AppointmentTypeDefinition>> GetEligibleByAgeAsync(
        int patientAgeInYears,
        CancellationToken ct = default
    );

    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

    Task<AppointmentTypeDefinition?> GetByIdIncludingDeletedAsync(
        Guid id,
        CancellationToken ct = default
    );

    Task<bool> ExistsByNameExcludingAsync(
        string name,
        Guid excludeId,
        CancellationToken ct = default
    );

    Task<AppointmentTypeDefinition> CreateAsync(
        AppointmentTypeDefinition appointmentType,
        CancellationToken ct = default
    );
}

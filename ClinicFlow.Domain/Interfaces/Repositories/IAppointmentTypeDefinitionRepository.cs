using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="AppointmentTypeDefinition"/> persistence operations.
/// </summary>
public interface IAppointmentTypeDefinitionRepository
{
    Task<AppointmentTypeDefinition?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<AppointmentTypeDefinition>> GetAllActiveAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<AppointmentTypeDefinition>> GetByCategoryAsync(
        AppointmentCategory category,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<AppointmentTypeDefinition>> GetEligibleByAgeAsync(
        int patientAgeInYears,
        CancellationToken cancellationToken = default
    );

    Task<AppointmentTypeDefinition> CreateAsync(
        AppointmentTypeDefinition appointmentType,
        CancellationToken cancellationToken = default
    );
}

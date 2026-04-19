using ClinicFlow.Domain.Entities;

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

    Task<AppointmentTypeDefinition> CreateAsync(
        AppointmentTypeDefinition appointmentType,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        AppointmentTypeDefinition appointmentType,
        CancellationToken cancellationToken = default
    );
}

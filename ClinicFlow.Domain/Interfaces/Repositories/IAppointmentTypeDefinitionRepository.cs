using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="AppointmentTypeDefinition"/> persistence operations.
/// </summary>
public interface IAppointmentTypeDefinitionRepository
{
    Task<AppointmentTypeDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IList<AppointmentTypeDefinition>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AppointmentTypeDefinition> CreateAsync(AppointmentTypeDefinition appointmentTypeDefinition, CancellationToken cancellationToken = default);
}

using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="AppointmentTypeDefinition"/> persistence operations.
/// </summary>
public interface IAppointmentTypeDefinitionRepository
{
    /// <summary>
    /// Retrieves an appointment type definition by its unique identifier.
    /// </summary>
    Task<AppointmentTypeDefinition?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all appointment type definitions.
    /// </summary>
    Task<IList<AppointmentTypeDefinition>> GetAllAsync();

    /// <summary>
    /// Persists a new appointment type definition.
    /// </summary>
    Task<AppointmentTypeDefinition> CreateAsync(AppointmentTypeDefinition appointmentTypeDefinition);
}

using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces;

public interface IAppointmentTypeDefinitionRepository
{
    Task<AppointmentTypeDefinition?> GetByIdAsync(Guid id);
    Task<IList<AppointmentTypeDefinition>> GetAllAsync();
    Task<AppointmentTypeDefinition> CreateAsync(AppointmentTypeDefinition appointmentTypeDefinition);
}

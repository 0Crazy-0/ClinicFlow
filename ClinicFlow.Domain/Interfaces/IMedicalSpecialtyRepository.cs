using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces;

public interface IMedicalSpecialtyRepository
{
    Task<MedicalSpecialty?> GetByIdAsync(Guid id);
}

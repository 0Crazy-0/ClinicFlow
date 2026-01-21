using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces
{
    public interface IDoctorRepository
    {
        Task<Doctor?> GetByIdAsync(Guid id);
        Task<Doctor?> GetByUserIdAsync(Guid userId);
        Task<List<Doctor>> GetBySpecialtyIdAsync(Guid specialtyId);
        Task<List<Doctor>> GetAllAsync();
        Task<Doctor> CreateAsync(Doctor doctor);
        Task UpdateAsync(Doctor doctor);
        Task<bool> ExistsByLicenseNumberAsync(string licenseNumber);
    }
}
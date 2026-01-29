using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Entities;

public class User : BaseEntity
{
    public Guid? DoctorId { get; private set; }
    public Guid? PatientId { get; private set; }
    public UserRoleEnum Role { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public User()
    {
        IsActive = true;
    }
}
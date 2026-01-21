using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
    public User()
    {
        IsActive = true;
    }
}
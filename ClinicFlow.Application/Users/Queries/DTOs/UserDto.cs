using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Users.Queries.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string PhoneNumber,
    UserRole Role,
    bool IsActive,
    DateTime? LastLoginAt,
    int FailedLoginAttempts,
    DateTime? LockoutEnd
);

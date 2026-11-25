using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Users.Queries.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string PhoneNumber,
    UserRole Role,
    bool IsActive,
    DateTimeOffset? LastLoginAt,
    int FailedLoginAttempts,
    DateTimeOffset? LockoutEnd
);

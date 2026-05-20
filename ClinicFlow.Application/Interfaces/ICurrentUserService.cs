using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's details.
/// </summary>
public interface ICurrentUserService
{
    Guid Id { get; }

    string Email { get; }

    UserRole Role { get; }

    bool IsAuthenticated { get; }
}

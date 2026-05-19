using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Interfaces;

public interface ICurrentUserService
{
    Guid Id { get; }
    string Email { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}

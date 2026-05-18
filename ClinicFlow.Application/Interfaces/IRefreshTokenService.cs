namespace ClinicFlow.Application.Interfaces;

public interface IRefreshTokenService
{
    Task RevokeAsync(Guid userId, CancellationToken cancellationToken = default);
}

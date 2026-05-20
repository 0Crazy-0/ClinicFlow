namespace ClinicFlow.Application.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>
    /// Revokes all active refresh tokens for the specified user.
    /// </summary>
    Task RevokeAsync(Guid userId, CancellationToken cancellationToken = default);
}

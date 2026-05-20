namespace ClinicFlow.Application.Interfaces;

/// <summary>
/// Manages generation and validation of password reset tokens.
/// </summary>
public interface IPasswordResetTokenService
{
    /// <summary>
    /// Generates a secure, temporary password reset token for the specified user.
    /// </summary>
    Task<string> GenerateTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a password reset token and returns the corresponding user's identifier.
    /// </summary>
    /// <returns>The user ID associated with the token, or null if the token is invalid or expired.</returns>
    Task<Guid?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}

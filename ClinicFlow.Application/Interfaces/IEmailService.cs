namespace ClinicFlow.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Sends a password reset verification link to the specified email.
    /// </summary>
    /// <param name="resetToken">The token used to verify the reset request.</param>
    Task SendPasswordResetEmailAsync(
        string email,
        string resetToken,
        CancellationToken cancellationToken = default
    );
}

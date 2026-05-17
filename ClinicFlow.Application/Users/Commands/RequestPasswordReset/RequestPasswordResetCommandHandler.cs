using ClinicFlow.Application.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.RequestPasswordReset;

/// <remarks>
/// Silently returns if the email does not exist. This prevents email enumeration attacks.
/// </remarks>
public sealed class RequestPasswordResetCommandHandler(
    IUserRepository userRepository,
    IPasswordResetTokenService passwordResetTokenService,
    IEmailService emailService
) : IRequestHandler<RequestPasswordResetCommand>
{
    public async Task Handle(
        RequestPasswordResetCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
            return;

        var token = await passwordResetTokenService.GenerateTokenAsync(user.Id, cancellationToken);

        await emailService.SendPasswordResetEmailAsync(request.Email, token, cancellationToken);
    }
}

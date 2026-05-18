using ClinicFlow.Application.Interfaces;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.LogoutUser;

public sealed class LogoutUserCommandHandler(IRefreshTokenService refreshTokenService)
    : IRequestHandler<LogoutUserCommand>
{
    public async Task Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeAsync(request.UserId, cancellationToken);
    }
}

using MediatR;

namespace ClinicFlow.Application.Users.Commands.LogoutUser;

public sealed record LogoutUserCommand(Guid UserId) : IRequest;

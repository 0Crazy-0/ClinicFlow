using MediatR;

namespace ClinicFlow.Application.Users.Commands.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : IRequest<Guid>;

using MediatR;

namespace ClinicFlow.Application.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid UserId) : IRequest;

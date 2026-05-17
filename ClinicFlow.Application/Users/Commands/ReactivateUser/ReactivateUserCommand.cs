using MediatR;

namespace ClinicFlow.Application.Users.Commands.ReactivateUser;

public sealed record ReactivateUserCommand(Guid UserId) : IRequest;

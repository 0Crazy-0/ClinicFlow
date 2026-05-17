using MediatR;

namespace ClinicFlow.Application.Users.Commands.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : IRequest;

using MediatR;

namespace ClinicFlow.Application.Users.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : IRequest;

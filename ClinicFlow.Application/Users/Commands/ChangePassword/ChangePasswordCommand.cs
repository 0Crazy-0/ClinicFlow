using MediatR;

namespace ClinicFlow.Application.Users.Commands.ChangePassword;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
    : IRequest;

using MediatR;

namespace ClinicFlow.Application.Users.Commands.RegisterAdminUser;

public sealed record RegisterAdminUserCommand(string Email, string Password, string PhoneNumber)
    : IRequest<Guid>;

using MediatR;

namespace ClinicFlow.Application.Users.Commands.RegisterReceptionistUser;

public sealed record RegisterReceptionistUserCommand(
    string Email,
    string Password,
    string PhoneNumber
) : IRequest<Guid>;

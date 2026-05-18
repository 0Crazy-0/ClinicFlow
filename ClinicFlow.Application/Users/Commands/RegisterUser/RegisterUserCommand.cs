using ClinicFlow.Application.Users.Commands.Shared.Register;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Password, string PhoneNumber)
    : IRequest<Guid>,
        IRegisterUserCommand;

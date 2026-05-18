using ClinicFlow.Application.Users.Commands.Shared.Register;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.RegisterDoctorUser;

public sealed record RegisterDoctorUserCommand(string Email, string Password, string PhoneNumber)
    : IRequest<Guid>,
        IRegisterUserCommand;

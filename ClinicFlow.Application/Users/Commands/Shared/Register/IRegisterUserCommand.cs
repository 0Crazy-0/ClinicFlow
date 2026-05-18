namespace ClinicFlow.Application.Users.Commands.Shared.Register;

public interface IRegisterUserCommand
{
    string Email { get; }
    string Password { get; }
    string PhoneNumber { get; }
}

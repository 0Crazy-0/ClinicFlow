namespace ClinicFlow.Application.Users.Commands.Shared.Register;

/// <summary>
/// Defines the common structure for commands that register a new user in the system.
/// </summary>
public interface IRegisterUserCommand
{
    string Email { get; }

    string Password { get; }

    string PhoneNumber { get; }
}

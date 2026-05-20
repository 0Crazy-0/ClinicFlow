namespace ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;

/// <summary>
/// Defines the common structure for commands that create a basic patient profile.
/// </summary>
public interface ICreatePatientCommand
{
    /// <summary>
    /// Gets the unique identifier of the user account associated with the patient.
    /// </summary>
    Guid UserId { get; }

    string FirstName { get; }

    string LastName { get; }

    DateTime DateOfBirth { get; }
}

namespace ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;

public interface ICreatePatientCommand
{
    Guid UserId { get; }
    string FirstName { get; }
    string LastName { get; }
    DateTime DateOfBirth { get; }
}

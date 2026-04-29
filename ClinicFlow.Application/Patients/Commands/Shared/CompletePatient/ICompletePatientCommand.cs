namespace ClinicFlow.Application.Patients.Commands.Shared.CompletePatient;

public interface ICompletePatientCommand
{
    Guid UserId { get; }
    string FirstName { get; }
    string LastName { get; }
    DateTime DateOfBirth { get; }
    string BloodType { get; }
    string EmergencyContactName { get; }
    string EmergencyContactPhone { get; }
}

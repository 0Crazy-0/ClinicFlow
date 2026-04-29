using ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;

namespace ClinicFlow.Application.Patients.Commands.Shared.CompletePatient;

public interface ICompletePatientCommand : ICreatePatientCommand
{
    string BloodType { get; }
    string EmergencyContactName { get; }
    string EmergencyContactPhone { get; }
}

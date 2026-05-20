using ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;

namespace ClinicFlow.Application.Patients.Commands.Shared.CompletePatient;

/// <summary>
/// Defines the common structure for commands that complete a patient profile with medical metadata.
/// </summary>
public interface ICompletePatientCommand : ICreatePatientCommand
{
    /// <summary>
    /// Gets the patient's blood type (e.g. "O+", "A-").
    /// </summary>
    string BloodType { get; }

    string EmergencyContactName { get; }

    string EmergencyContactPhone { get; }
}

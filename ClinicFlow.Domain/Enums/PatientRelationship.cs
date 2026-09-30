namespace ClinicFlow.Domain.Enums;

/// <summary>
/// Defines the relationship of the user to the patient profile.
/// </summary>
public enum PatientRelationship
{
    Self = 0,
    Child = 1,
    Spouse = 2,
    Parent = 3,
    Sibling = 4,
    Other = 5,
}

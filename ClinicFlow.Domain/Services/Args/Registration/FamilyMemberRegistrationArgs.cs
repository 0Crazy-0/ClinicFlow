using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Registration;

public sealed record FamilyMemberRegistrationArgs
{
    public Guid UserId { get; init; }
    public required PersonName FullName { get; init; }
    public required PatientRelationship Relationship { get; init; }
    public DateTime DateOfBirth { get; init; }
    public DateTime ReferenceTime { get; init; }
}

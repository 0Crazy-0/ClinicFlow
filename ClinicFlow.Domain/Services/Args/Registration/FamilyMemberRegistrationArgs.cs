using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Registration;

public sealed record FamilyMemberRegistrationArgs
{
    public PatientRelationship Role { get; init; }
    public LegalAuthorityType LegalAuthority { get; init; }
    public FamilyMembershipAccessLevel AccessLevel { get; init; }
    public required PersonName FullName { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public BiologicalSex SexAtBirth { get; init; }
}

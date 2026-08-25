using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Registration;

public sealed record FamilyMemberRegistrationArgs
{
    public Patient? ExistingPatient { get; init; }
    public bool HasExistingMembershipWithOwner { get; init; }
    public int ActiveFamilyMemberCount { get; init; }
    public int OwnerAgeInYears { get; init; }
    public required Guid OwnerUserId { get; init; }
    public required PatientRelationship Role { get; init; }
    public required PersonName FullName { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public DateTime ReferenceTime { get; init; }
}

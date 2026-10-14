using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Registration;

public sealed record PrimaryProfileRegistrationArgs
{
    public Patient? ExistingPatient { get; init; }
    public bool HasExistingSelfMembership { get; init; }
    public Guid UserId { get; init; }
    public required PersonName FullName { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public BiologicalSex SexAtBirth { get; init; }
    public DateTime ReferenceTime { get; init; }
}

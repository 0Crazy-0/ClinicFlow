using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Registration;

public sealed record PrimaryProfileRegistrationArgs
{
    public Guid UserId { get; init; }
    public required PersonName FullName { get; init; }
    public DateTime DateOfBirth { get; init; }
    public DateTime ReferenceTime { get; init; }
}

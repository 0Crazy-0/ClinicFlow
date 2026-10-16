using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.Registration;

public sealed record class PrimaryProfileRegistrationContext
{
    public Patient? ExistingPatient { get; init; }
    public bool HasExistingSelfMembership { get; init; }
    public Guid UserId { get; init; }
    public DateTime ReferenceTime { get; init; }
}

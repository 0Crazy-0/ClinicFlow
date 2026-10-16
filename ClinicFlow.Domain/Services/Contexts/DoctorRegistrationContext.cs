using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record class DoctorRegistrationContext
{
    public Doctor? ExistingDoctor { get; init; }
    public Guid UserId { get; init; }
}

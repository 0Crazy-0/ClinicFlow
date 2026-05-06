using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record DoctorRegistrationContext
{
    public Doctor? ExistingDoctor { get; init; }
}

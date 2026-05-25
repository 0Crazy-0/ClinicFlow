using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Registration;

public sealed record DoctorRegistrationArgs
{
    public Guid UserId { get; init; }
    public required PersonName FullName { get; init; }
    public required MedicalLicenseNumber LicenseNumber { get; init; }
    public Guid MedicalSpecialtyId { get; init; }
    public required string Biography { get; init; }
    public required ConsultationRoom ConsultationRoom { get; init; }
}

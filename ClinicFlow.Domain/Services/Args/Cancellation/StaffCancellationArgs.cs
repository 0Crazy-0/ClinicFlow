using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public record StaffCancellationArgs(
    Guid InitiatorUserId,
    UserRole Role,
    MedicalSpecialty Specialty,
    string Reason
);

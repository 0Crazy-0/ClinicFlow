using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public record PatientCancellationArgs(
    Patient AppointmentPatient,
    Patient? InitiatorPatient,
    AppointmentCategory Category,
    MedicalSpecialty Specialty,
    string? Reason
);

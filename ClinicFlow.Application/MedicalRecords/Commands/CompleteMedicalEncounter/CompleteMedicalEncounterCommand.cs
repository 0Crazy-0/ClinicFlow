using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;

public record DynamicClinicalDetailDto(string TemplateCode, string JsonDataPayload);

public sealed record CompleteMedicalEncounterCommand(
    Guid PatientId,
    Guid DoctorId,
    Guid AppointmentId,
    string ChiefComplaint,
    IReadOnlyList<DynamicClinicalDetailDto> Details
) : IRequest<Guid>;

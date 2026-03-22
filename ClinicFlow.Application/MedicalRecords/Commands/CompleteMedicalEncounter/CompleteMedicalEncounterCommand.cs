using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;

public record DynamicClinicalDetailDto(string TemplateCode, string JsonDataPayload);

public record CompleteMedicalEncounterCommand(
    Guid PatientId,
    Guid DoctorId,
    Guid AppointmentId,
    string ChiefComplaint,
    IEnumerable<DynamicClinicalDetailDto> Details
) : IRequest<Guid>;

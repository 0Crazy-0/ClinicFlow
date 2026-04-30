using MediatR;
using DynamicClinicalDetailDto = (string TemplateCode, string JsonDataPayload);

namespace ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;

public sealed record CompleteMedicalEncounterCommand(
    Guid PatientId,
    Guid DoctorId,
    Guid AppointmentId,
    string ChiefComplaint,
    IReadOnlyList<DynamicClinicalDetailDto> Details
) : IRequest<Guid>;

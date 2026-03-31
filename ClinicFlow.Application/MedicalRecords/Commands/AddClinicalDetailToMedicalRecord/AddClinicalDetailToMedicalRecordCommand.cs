using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;

public record AddClinicalDetailToMedicalRecordCommand(
    Guid MedicalRecordId,
    string TemplateCode,
    string JsonDataPayload
) : IRequest;

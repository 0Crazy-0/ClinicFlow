using ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;

public record AddClinicalDetailToMedicalRecordCommand(
    Guid MedicalRecordId,
    DynamicClinicalDetailDto Detail
) : IRequest;

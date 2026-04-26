using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetClinicalDetailByTemplateCode;

public sealed record GetClinicalDetailByTemplateCodeQuery(Guid MedicalRecordId, string TemplateCode)
    : IRequest<ClinicalDetailDto?>;

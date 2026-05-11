using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.GetClinicalFormTemplateByCode;

public sealed record GetClinicalFormTemplateByCodeQuery(string Code)
    : IRequest<ClinicalFormTemplateDto>;

using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.GetClinicalFormTemplateById;

public sealed record GetClinicalFormTemplateByIdQuery(Guid ClinicalFormTemplateId)
    : IRequest<ClinicalFormTemplateDto>;

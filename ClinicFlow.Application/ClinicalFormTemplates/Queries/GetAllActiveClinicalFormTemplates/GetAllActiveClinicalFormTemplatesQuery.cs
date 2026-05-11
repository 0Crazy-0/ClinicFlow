using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.GetAllActiveClinicalFormTemplates;

public sealed record GetAllActiveClinicalFormTemplatesQuery
    : IRequest<IReadOnlyList<ClinicalFormTemplateDto>>;

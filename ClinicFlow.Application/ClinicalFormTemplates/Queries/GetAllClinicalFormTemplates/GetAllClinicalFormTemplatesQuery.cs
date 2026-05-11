using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.GetAllClinicalFormTemplates;

public sealed record GetAllClinicalFormTemplatesQuery
    : IRequest<IReadOnlyList<ClinicalFormTemplateDto>>;

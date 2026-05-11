using ClinicFlow.Application.MedicalSpecialties.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Queries.GetActiveMedicalSpecialties;

public sealed record GetActiveMedicalSpecialtiesQuery
    : IRequest<IReadOnlyList<MedicalSpecialtyDto>>;

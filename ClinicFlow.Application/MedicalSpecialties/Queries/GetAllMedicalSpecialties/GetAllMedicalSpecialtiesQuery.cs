using ClinicFlow.Application.MedicalSpecialties.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Queries.GetAllMedicalSpecialties;

public sealed record GetAllMedicalSpecialtiesQuery : IRequest<IReadOnlyList<MedicalSpecialtyDto>>;

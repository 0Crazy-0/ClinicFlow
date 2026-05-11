using ClinicFlow.Application.MedicalSpecialties.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Queries.GetMedicalSpecialtyById;

public sealed record GetMedicalSpecialtyByIdQuery(Guid MedicalSpecialtyId)
    : IRequest<MedicalSpecialtyDto>;

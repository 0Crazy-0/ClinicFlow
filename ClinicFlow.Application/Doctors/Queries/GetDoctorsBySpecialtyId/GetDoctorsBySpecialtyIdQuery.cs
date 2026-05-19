using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Doctors.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorsBySpecialtyId;

public sealed record GetDoctorsBySpecialtyIdQuery(Guid SpecialtyId, int PageNumber, int PageSize)
    : IRequest<PaginatedList<DoctorDto>>;

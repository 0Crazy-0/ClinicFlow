using ClinicFlow.Application.Doctors.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorsBySpecialtyId;

public sealed record GetDoctorsBySpecialtyIdQuery(Guid SpecialtyId)
    : IRequest<IReadOnlyList<DoctorDto>>;

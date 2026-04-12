using ClinicFlow.Application.Doctors.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorById;

public sealed record GetDoctorByIdQuery(Guid DoctorId) : IRequest<DoctorDto>;

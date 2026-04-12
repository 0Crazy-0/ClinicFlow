using ClinicFlow.Application.Doctors.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Doctors.Queries.GetDoctorByUserId;

public sealed record GetDoctorByUserIdQuery(Guid UserId) : IRequest<DoctorDto>;

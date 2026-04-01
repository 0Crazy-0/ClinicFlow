using ClinicFlow.Application.Patients.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.GetPatientsByUserId;

public sealed record GetPatientsByUserIdQuery(Guid UserId) : IRequest<IReadOnlyList<PatientDto>>;

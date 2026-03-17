using ClinicFlow.Application.Patients.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.GetPatientsByUserId;

public record GetPatientsByUserIdQuery(Guid UserId) : IRequest<IEnumerable<PatientDto>>;

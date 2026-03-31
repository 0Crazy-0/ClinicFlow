using ClinicFlow.Application.Patients.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Patients.Queries.GetPatientById;

public sealed record GetPatientByIdQuery(Guid PatientId) : IRequest<PatientDto>;

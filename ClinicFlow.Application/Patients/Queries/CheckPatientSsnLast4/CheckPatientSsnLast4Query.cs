using MediatR;

namespace ClinicFlow.Application.Patients.Queries.CheckPatientSsnLast4;

public sealed record CheckPatientSsnLast4Query(Guid PatientId, string Last4) : IRequest<bool>;

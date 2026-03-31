using ClinicFlow.Application.Appointments.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByPatientId;

public sealed record GetAppointmentsByPatientIdQuery(Guid PatientId)
    : IRequest<IEnumerable<AppointmentDto>>;

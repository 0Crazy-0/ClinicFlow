using ClinicFlow.Application.Appointments.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;

public sealed record GetAppointmentsByDoctorIdQuery(Guid DoctorId, DateTime Date)
    : IRequest<IEnumerable<AppointmentDto>>;

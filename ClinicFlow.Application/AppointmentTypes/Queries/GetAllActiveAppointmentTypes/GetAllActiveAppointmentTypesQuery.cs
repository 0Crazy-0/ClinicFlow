using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetAllActiveAppointmentTypes;

public sealed record GetAllActiveAppointmentTypesQuery
    : IRequest<IReadOnlyList<AppointmentTypeDto>>;

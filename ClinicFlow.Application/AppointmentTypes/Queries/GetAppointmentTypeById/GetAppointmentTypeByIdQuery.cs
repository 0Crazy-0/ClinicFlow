using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypeById;

public sealed record GetAppointmentTypeByIdQuery(Guid AppointmentTypeId)
    : IRequest<AppointmentTypeDto>;

using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByCategory;

public sealed record GetAppointmentTypesByCategoryQuery(AppointmentCategory Category)
    : IRequest<IReadOnlyList<AppointmentTypeDto>>;

using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByPurpose;

public sealed record GetAppointmentTypesByPurposeQuery(AppointmentPurpose Purpose)
    : IRequest<IReadOnlyList<AppointmentTypeDto>>;

using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetEligibleAppointmentTypes;

public sealed record GetEligibleAppointmentTypesQuery(int PatientAgeInYears)
    : IRequest<IReadOnlyList<AppointmentTypeDto>>;

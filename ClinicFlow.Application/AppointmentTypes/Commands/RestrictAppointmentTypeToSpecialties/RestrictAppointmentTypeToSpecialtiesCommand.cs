using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.RestrictAppointmentTypeToSpecialties;

public sealed record RestrictAppointmentTypeToSpecialtiesCommand(
    Guid AppointmentTypeId,
    IReadOnlyCollection<Guid> SpecialtyIds
) : IRequest;

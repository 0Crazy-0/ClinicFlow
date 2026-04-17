using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Penalties.Commands.BlockPatient;

public sealed record BlockPatientCommand(Guid PatientId, string Reason, BlockDuration Duration)
    : IRequest<Guid>;

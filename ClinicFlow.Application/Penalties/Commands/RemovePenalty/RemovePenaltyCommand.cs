using MediatR;

namespace ClinicFlow.Application.Penalties.Commands.RemovePenalty;

public sealed record RemovePenaltyCommand(Guid PenaltyId) : IRequest;

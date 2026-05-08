using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.CreateMedicalSpecialty;

public sealed record CreateMedicalSpecialtyCommand(
    string Name,
    string Description,
    int TypicalDurationMinutes,
    int MinCancellationHours
) : IRequest<Guid>;

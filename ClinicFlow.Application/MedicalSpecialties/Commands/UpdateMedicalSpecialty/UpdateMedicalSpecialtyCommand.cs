using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.UpdateMedicalSpecialty;

public sealed record UpdateMedicalSpecialtyCommand(
    Guid SpecialtyId,
    string Name,
    string Description,
    int TypicalDurationMinutes,
    int MinCancellationHours
) : IRequest;

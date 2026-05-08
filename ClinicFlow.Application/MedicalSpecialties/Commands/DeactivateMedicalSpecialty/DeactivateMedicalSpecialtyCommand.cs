using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.DeactivateMedicalSpecialty;

public sealed record DeactivateMedicalSpecialtyCommand(Guid SpecialtyId) : IRequest;

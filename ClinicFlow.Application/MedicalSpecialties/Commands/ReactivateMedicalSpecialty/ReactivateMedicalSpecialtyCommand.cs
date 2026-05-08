using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.ReactivateMedicalSpecialty;

public sealed record ReactivateMedicalSpecialtyCommand(Guid SpecialtyId) : IRequest;

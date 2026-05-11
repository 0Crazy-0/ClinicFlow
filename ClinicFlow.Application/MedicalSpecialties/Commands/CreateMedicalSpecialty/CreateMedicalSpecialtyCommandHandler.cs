using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.CreateMedicalSpecialty;

public sealed class CreateMedicalSpecialtyCommandHandler(
    IMedicalSpecialtyRepository medicalSpecialtyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateMedicalSpecialtyCommand, Guid>
{
    public async Task<Guid> Handle(CreateMedicalSpecialtyCommand request, CancellationToken ct)
    {
        if (await medicalSpecialtyRepository.ExistsByNameAsync(request.Name, ct))
            throw new BusinessRuleValidationException(
                DomainErrors.MedicalSpecialty.NameAlreadyExists
            );

        var specialty = MedicalSpecialty.Create(
            request.Name,
            request.Description,
            request.TypicalDurationMinutes,
            request.MinCancellationHours
        );

        await medicalSpecialtyRepository.CreateAsync(specialty, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return specialty.Id;
    }
}

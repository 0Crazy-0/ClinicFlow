using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.CreateAppointmentType;

public sealed class CreateAppointmentTypeCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateAppointmentTypeCommand, Guid>
{
    public async Task<Guid> Handle(CreateAppointmentTypeCommand request, CancellationToken ct)
    {
        if (await appointmentTypeRepository.ExistsByNameAsync(request.Name, ct))
            throw new BusinessRuleValidationException(
                DomainErrors.AppointmentType.NameAlreadyExists
            );

        var agePolicy = AgeEligibilityPolicy.Create(
            request.MinimumAge,
            request.MaximumAge,
            request.RequiresGuardianConsent
        );

        var appointmentType = AppointmentTypeDefinition.Create(
            request.Category,
            request.Name,
            request.Description,
            request.DurationMinutes,
            agePolicy
        );

        await appointmentTypeRepository.CreateAsync(appointmentType, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return appointmentType.Id;
    }
}

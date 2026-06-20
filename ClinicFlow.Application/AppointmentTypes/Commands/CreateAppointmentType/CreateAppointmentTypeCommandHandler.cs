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
    /// <inheritdoc />
    public async Task<Guid> Handle(
        CreateAppointmentTypeCommand request,
        CancellationToken cancellationToken
    )
    {
        if (await appointmentTypeRepository.ExistsByNameAsync(request.Name, cancellationToken))
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
            EncounterDuration.FromMinutes(request.DurationMinutes),
            agePolicy
        );

        await appointmentTypeRepository.CreateAsync(appointmentType, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appointmentType.Id;
    }
}

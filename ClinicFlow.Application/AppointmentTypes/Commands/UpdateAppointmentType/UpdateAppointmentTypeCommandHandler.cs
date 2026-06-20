using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.UpdateAppointmentType;

public sealed class UpdateAppointmentTypeCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateAppointmentTypeCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        UpdateAppointmentTypeCommand request,
        CancellationToken cancellationToken
    )
    {
        var appointmentType =
            await appointmentTypeRepository.GetByIdAsync(
                request.AppointmentTypeId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                request.AppointmentTypeId
            );

        if (
            await appointmentTypeRepository.ExistsByNameExcludingAsync(
                request.Name,
                request.AppointmentTypeId,
                cancellationToken
            )
        )
            throw new BusinessRuleValidationException(
                DomainErrors.AppointmentType.NameAlreadyExists
            );

        appointmentType.UpdateDetails(
            request.Category,
            request.Name,
            request.Description,
            EncounterDuration.FromMinutes(request.DurationMinutes)
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

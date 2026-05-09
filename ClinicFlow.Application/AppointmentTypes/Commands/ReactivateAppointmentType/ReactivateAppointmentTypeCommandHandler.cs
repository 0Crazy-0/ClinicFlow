using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.ReactivateAppointmentType;

public sealed class ReactivateAppointmentTypeCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReactivateAppointmentTypeCommand>
{
    public async Task Handle(
        ReactivateAppointmentTypeCommand request,
        CancellationToken cancellationToken
    )
    {
        var appointmentType =
            await appointmentTypeRepository.GetByIdIncludingDeletedAsync(
                request.AppointmentTypeId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                request.AppointmentTypeId
            );

        if (
            await appointmentTypeRepository.ExistsByNameAsync(
                appointmentType.Name,
                cancellationToken
            )
        )
            throw new BusinessRuleValidationException(
                DomainErrors.AppointmentType.NameAlreadyExists
            );

        appointmentType.Reactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

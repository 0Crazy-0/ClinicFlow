using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.DeactivateAppointmentType;

public sealed class DeactivateAppointmentTypeCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeactivateAppointmentTypeCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        DeactivateAppointmentTypeCommand request,
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

        appointmentType.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.DeleteAppointmentType;

public sealed class DeleteAppointmentTypeCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteAppointmentTypeCommand>
{
    public async Task Handle(
        DeleteAppointmentTypeCommand request,
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

        appointmentType.MarkAsDeleted();

        await appointmentTypeRepository.UpdateAsync(appointmentType, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

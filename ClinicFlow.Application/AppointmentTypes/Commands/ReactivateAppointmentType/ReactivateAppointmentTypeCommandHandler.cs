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
    public async Task Handle(ReactivateAppointmentTypeCommand request, CancellationToken ct)
    {
        var appointmentType =
            await appointmentTypeRepository.GetByIdIncludingDeletedAsync(
                request.AppointmentTypeId,
                ct
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                request.AppointmentTypeId
            );

        if (await appointmentTypeRepository.ExistsByNameAsync(appointmentType.Name, ct))
            throw new BusinessRuleValidationException(
                DomainErrors.AppointmentType.NameAlreadyExists
            );

        appointmentType.Reactivate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}

using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.UpdateAppointmentType;

public sealed class UpdateAppointmentTypeCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateAppointmentTypeCommand>
{
    public async Task Handle(UpdateAppointmentTypeCommand request, CancellationToken ct)
    {
        var appointmentType =
            await appointmentTypeRepository.GetByIdAsync(request.AppointmentTypeId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                request.AppointmentTypeId
            );

        if (
            await appointmentTypeRepository.ExistsByNameExcludingAsync(
                request.Name,
                request.AppointmentTypeId,
                ct
            )
        )
            throw new BusinessRuleValidationException(
                DomainErrors.AppointmentType.NameAlreadyExists
            );

        appointmentType.UpdateDetails(
            request.Category,
            request.Name,
            request.Description,
            request.DurationMinutes
        );

        await unitOfWork.SaveChangesAsync(ct);
    }
}

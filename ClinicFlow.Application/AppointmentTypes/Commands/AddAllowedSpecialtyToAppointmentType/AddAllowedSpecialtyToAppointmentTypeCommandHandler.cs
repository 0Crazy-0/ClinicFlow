using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.AddAllowedSpecialtyToAppointmentType;

public sealed class AddAllowedSpecialtyToAppointmentTypeCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddAllowedSpecialtyToAppointmentTypeCommand>
{
    public async Task Handle(
        AddAllowedSpecialtyToAppointmentTypeCommand request,
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

        appointmentType.AddAllowedSpecialty(request.SpecialtyId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

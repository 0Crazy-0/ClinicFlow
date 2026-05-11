using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.RemoveAllowedSpecialtyFromAppointmentType;

public sealed class RemoveAllowedSpecialtyFromAppointmentTypeCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RemoveAllowedSpecialtyFromAppointmentTypeCommand>
{
    public async Task Handle(
        RemoveAllowedSpecialtyFromAppointmentTypeCommand request,
        CancellationToken ct
    )
    {
        var appointmentType =
            await appointmentTypeRepository.GetByIdAsync(request.AppointmentTypeId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                request.AppointmentTypeId
            );

        appointmentType.RemoveAllowedSpecialty(request.SpecialtyId);

        await unitOfWork.SaveChangesAsync(ct);
    }
}

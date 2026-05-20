using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.MakeAppointmentTypeUnrestricted;

public sealed class MakeAppointmentTypeUnrestrictedCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<MakeAppointmentTypeUnrestrictedCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        MakeAppointmentTypeUnrestrictedCommand request,
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

        appointmentType.MakeUnrestricted();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

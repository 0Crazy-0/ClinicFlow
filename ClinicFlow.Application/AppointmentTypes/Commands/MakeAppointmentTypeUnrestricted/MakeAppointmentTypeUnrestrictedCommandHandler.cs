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
    public async Task Handle(MakeAppointmentTypeUnrestrictedCommand request, CancellationToken ct)
    {
        var appointmentType =
            await appointmentTypeRepository.GetByIdAsync(request.AppointmentTypeId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                request.AppointmentTypeId
            );

        appointmentType.MakeUnrestricted();

        await unitOfWork.SaveChangesAsync(ct);
    }
}

using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.RestrictAppointmentTypeToSpecialties;

public sealed class RestrictAppointmentTypeToSpecialtiesCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RestrictAppointmentTypeToSpecialtiesCommand>
{
    public async Task Handle(
        RestrictAppointmentTypeToSpecialtiesCommand request,
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

        appointmentType.RestrictToSpecialties(request.SpecialtyIds);

        await unitOfWork.SaveChangesAsync(ct);
    }
}

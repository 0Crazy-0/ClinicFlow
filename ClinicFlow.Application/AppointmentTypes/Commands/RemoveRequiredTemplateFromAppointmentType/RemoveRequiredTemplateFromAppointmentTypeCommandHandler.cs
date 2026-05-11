using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.RemoveRequiredTemplateFromAppointmentType;

public sealed class RemoveRequiredTemplateFromAppointmentTypeCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IClinicalFormTemplateRepository clinicalFormTemplateRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RemoveRequiredTemplateFromAppointmentTypeCommand>
{
    public async Task Handle(
        RemoveRequiredTemplateFromAppointmentTypeCommand request,
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

        var template =
            await clinicalFormTemplateRepository.GetByIdAsync(request.TemplateId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(ClinicalFormTemplate),
                request.TemplateId
            );

        appointmentType.RemoveRequiredTemplate(template);

        await unitOfWork.SaveChangesAsync(ct);
    }
}

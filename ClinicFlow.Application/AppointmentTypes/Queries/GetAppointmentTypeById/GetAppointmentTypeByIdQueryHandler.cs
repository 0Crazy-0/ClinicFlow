using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypeById;

public sealed class GetAppointmentTypeByIdQueryHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository
) : IRequestHandler<GetAppointmentTypeByIdQuery, AppointmentTypeDto>
{
    /// <inheritdoc />
    public async Task<AppointmentTypeDto> Handle(
        GetAppointmentTypeByIdQuery request,
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

        return new AppointmentTypeDto(
            appointmentType.Id,
            appointmentType.Category.ToString(),
            appointmentType.Name,
            appointmentType.Description,
            appointmentType.Duration.Minutes,
            appointmentType.AgePolicy.MinimumAge,
            appointmentType.AgePolicy.MaximumAge,
            appointmentType.AgePolicy.RequiresLegalGuardian,
            appointmentType.IsUnrestrictedBySpecialty,
            appointmentType.AllowedSpecialtyIds,
            [
                .. appointmentType.RequiredTemplates.Select(template => new ClinicalFormTemplateDto(
                    template.Id,
                    template.Code,
                    template.Name,
                    template.Description,
                    template.JsonSchemaDefinition,
                    template.IsDeleted
                )),
            ]
        );
    }
}

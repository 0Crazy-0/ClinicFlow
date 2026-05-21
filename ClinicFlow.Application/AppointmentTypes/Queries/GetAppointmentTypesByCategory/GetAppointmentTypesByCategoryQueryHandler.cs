using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByCategory;

public sealed class GetAppointmentTypesByCategoryQueryHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository
) : IRequestHandler<GetAppointmentTypesByCategoryQuery, IReadOnlyList<AppointmentTypeDto>>
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<AppointmentTypeDto>> Handle(
        GetAppointmentTypesByCategoryQuery request,
        CancellationToken cancellationToken
    )
    {
        var appointmentTypes = await appointmentTypeRepository.GetByCategoryAsync(
            request.Category,
            cancellationToken
        );

        return
        [
            .. appointmentTypes.Select(appointmentType => new AppointmentTypeDto(
                appointmentType.Id,
                appointmentType.Category.ToString(),
                appointmentType.Name,
                appointmentType.Description,
                appointmentType.DurationMinutes,
                appointmentType.AgePolicy.MinimumAge,
                appointmentType.AgePolicy.MaximumAge,
                appointmentType.AgePolicy.RequiresLegalGuardian,
                appointmentType.IsUnrestrictedBySpecialty,
                appointmentType.AllowedSpecialtyIds,
                [
                    .. appointmentType.RequiredTemplates.Select(
                        template => new ClinicalFormTemplateDto(
                            template.Id,
                            template.Code,
                            template.Name,
                            template.Description,
                            template.JsonSchemaDefinition,
                            template.IsDeleted
                        )
                    ),
                ]
            )),
        ];
    }
}

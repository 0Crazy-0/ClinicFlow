using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByCategory;

public sealed class GetAppointmentTypesByCategoryQueryHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository
) : IRequestHandler<GetAppointmentTypesByCategoryQuery, IReadOnlyList<AppointmentTypeDto>>
{
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
                appointmentType.AgePolicy.RequiresLegalGuardian
            )),
        ];
    }
}

using ClinicFlow.Application.AppointmentTypes.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Queries.GetEligibleAppointmentTypes;

public sealed class GetEligibleAppointmentTypesQueryHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository
) : IRequestHandler<GetEligibleAppointmentTypesQuery, IReadOnlyList<AppointmentTypeDto>>
{
    public async Task<IReadOnlyList<AppointmentTypeDto>> Handle(
        GetEligibleAppointmentTypesQuery request,
        CancellationToken cancellationToken
    )
    {
        var appointmentTypes = await appointmentTypeRepository.GetEligibleByAgeAsync(
            request.PatientAgeInYears,
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

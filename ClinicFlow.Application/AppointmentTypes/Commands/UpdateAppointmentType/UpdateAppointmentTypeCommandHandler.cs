using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.AppointmentTypes.Commands.UpdateAppointmentType;

public sealed class UpdateAppointmentTypeCommandHandler(
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateAppointmentTypeCommand>
{
    public async Task Handle(
        UpdateAppointmentTypeCommand request,
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

        appointmentType.UpdateDetails(
            request.Category,
            request.Name,
            request.Description,
            request.DurationMinutes
        );

        var agePolicy = AgeEligibilityPolicy.Create(
            request.MinimumAge,
            request.MaximumAge,
            request.RequiresGuardianConsent
        );

        appointmentType.ChangeAgePolicy(agePolicy);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

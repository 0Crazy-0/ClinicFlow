using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.LeaveFamilyAccount;

public sealed class LeaveFamilyAccountCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<LeaveFamilyAccountCommand>
{
    /// <inheritdoc />
    public async Task Handle(LeaveFamilyAccountCommand request, CancellationToken cancellationToken)
    {
        var patient =
            await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.PatientId
            );

        var referenceDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        patient.LeaveFamilyAccount(request.InitiatorUserId, referenceDate);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

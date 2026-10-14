using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.CorrectBiologicalSexByDoctor;

public sealed class CorrectBiologicalSexByDoctorCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CorrectBiologicalSexByDoctorCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        CorrectBiologicalSexByDoctorCommand request,
        CancellationToken cancellationToken
    )
    {
        var patient =
            await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.PatientId
            );

        var initiatorDoctor =
            await doctorRepository.GetByUserIdAsync(request.InitiatorUserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.InitiatorUserId
            );

        patient.CorrectBiologicalSex(
            request.CorrectedSex,
            request.Reason,
            initiatorDoctor.Id,
            timeProvider.GetLocalNow().DateTime
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

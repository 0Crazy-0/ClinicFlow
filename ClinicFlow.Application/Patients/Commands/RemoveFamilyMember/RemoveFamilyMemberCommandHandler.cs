using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.RemoveFamilyMember;

public sealed class RemoveFamilyMemberCommandHandler(
    IPatientRepository patientRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RemoveFamilyMemberCommand>
{
    public async Task Handle(RemoveFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        var patient =
            await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.PatientId
            );

        patient.RemoveFamilyMember(request.UserId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

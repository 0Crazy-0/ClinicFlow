using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.CreatePatientProfile;

public sealed class CreatePatientProfileCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreatePatientProfileCommand, Guid>
{
    public async Task<Guid> Handle(
        CreatePatientProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var fullName = PersonName.Create($"{request.FirstName} {request.LastName}");

        var patient = Patient.CreateSelf(
            request.UserId,
            fullName,
            request.DateOfBirth,
            timeProvider.GetUtcNow().UtcDateTime
        );

        await patientRepository.CreateAsync(patient, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return patient.Id;
    }
}

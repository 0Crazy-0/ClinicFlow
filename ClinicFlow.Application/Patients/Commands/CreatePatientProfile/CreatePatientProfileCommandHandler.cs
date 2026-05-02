using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
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
        var existingProfile = await patientRepository.GetIncludingDeletedByNameAndDobAsync(
            request.UserId,
            fullName,
            request.DateOfBirth,
            cancellationToken
        );

        var patient = PrimaryProfileRegistrationService.Register(
            existingProfile,
            new PrimaryProfileRegistrationArgs
            {
                UserId = request.UserId,
                FullName = fullName,
                DateOfBirth = request.DateOfBirth,
                ReferenceTime = timeProvider.GetUtcNow().UtcDateTime,
            }
        );

        if (existingProfile is null)
            await patientRepository.CreateAsync(patient, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return patient.Id;
    }
}

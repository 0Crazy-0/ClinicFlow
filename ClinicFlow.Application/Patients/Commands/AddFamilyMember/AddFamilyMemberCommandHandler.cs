using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.AddFamilyMember;

public sealed class AddFamilyMemberCommandHandler(
    TimeProvider timeProvider,
    IPatientRepository patientRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddFamilyMemberCommand, Guid>
{
    public async Task<Guid> Handle(
        AddFamilyMemberCommand request,
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

        var patient = FamilyMemberRegistrationService.Register(
            existingProfile,
            new FamilyMemberRegistrationArgs
            {
                UserId = request.UserId,
                FullName = fullName,
                Relationship = request.Relationship,
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

using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Patients.Commands.ClosePatientAccount;

public sealed class ClosePatientAccountCommandHandler(
    IPatientRepository patientRepository,
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ClosePatientAccountCommand>
{
    public async Task Handle(
        ClosePatientAccountCommand request,
        CancellationToken cancellationToken
    )
    {
        var patients = await patientRepository.GetAllByUserIdAsync(
            request.UserId,
            cancellationToken
        );

        var primaryPatient = patients.Single(p => p.RelationshipToUser is PatientRelationship.Self);
        var familyMembers = patients
            .Where(p => p.RelationshipToUser is not PatientRelationship.Self)
            .ToList();

        var hasPendingAppointments = await appointmentRepository.HasActiveAppointmentsForUserAsync(
            request.UserId,
            cancellationToken
        );

        primaryPatient.CloseAccount(hasPendingAppointments);
        familyMembers.ForEach(member => member.RemoveFamilyMember(request.UserId));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

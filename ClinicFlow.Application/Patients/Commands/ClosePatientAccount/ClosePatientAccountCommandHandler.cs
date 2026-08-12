using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
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
    /// <inheritdoc />
    public async Task Handle(
        ClosePatientAccountCommand request,
        CancellationToken cancellationToken
    )
    {
        await unitOfWork.ExecuteWithLockAsync(
            request.UserId,
            async cancellationToken =>
            {
                if (
                    await appointmentRepository.HasActiveAppointmentsForUserAsync(
                        request.UserId,
                        cancellationToken
                    )
                )
                    throw new DomainValidationException(
                        DomainErrors.Patient.CannotCloseAccountWithPendingAppointments
                    );

                if (
                    await patientRepository.HasActiveFamilyMembersAsync(
                        request.UserId,
                        cancellationToken
                    )
                )
                    throw new ActiveFamilyMembersExistException(
                        DomainErrors.User.CannotCloseAccountWithActiveFamilyMembers,
                        request.UserId
                    );

                var primaryPatient =
                    await patientRepository.GetSelfPatientByUserIdAsync(
                        request.UserId,
                        cancellationToken
                    )
                    ?? throw new EntityNotFoundException(
                        DomainErrors.General.NotFound,
                        nameof(Patient),
                        request.UserId
                    );

                primaryPatient.CloseAccount();

                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken
        );
    }
}

using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.GuardianInvolvement;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.SetGuardianInvolvementDeterminationByDoctor;

public sealed class SetGuardianInvolvementDeterminationByDoctorCommandHandler(
    IMedicalRecordRepository medicalRecordRepository,
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetGuardianInvolvementDeterminationByDoctorCommand>
{
    /// <inheritdoc />
    public async Task Handle(
        SetGuardianInvolvementDeterminationByDoctorCommand request,
        CancellationToken cancellationToken
    )
    {
        var record =
            await medicalRecordRepository.GetByIdAsync(request.MedicalRecordId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalRecord),
                request.MedicalRecordId
            );

        var appointment =
            await appointmentRepository.GetByIdAsync(record.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                record.AppointmentId
            );

        var initiatorDoctor =
            await doctorRepository.GetByUserIdAsync(request.InitiatorUserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.InitiatorUserId
            );

        MedicalEncounterService.RecordGuardianInvolvementDetermination(
            record,
            appointment,
            new DoctorGuardianInvolvementArgs
            {
                InitiatorDoctorId = initiatorDoctor.Id,
                GuardianInvolvementDeemedAppropriate = request.GuardianInvolvementDeemedAppropriate,
            }
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

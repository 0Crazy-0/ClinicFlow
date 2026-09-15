using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.StartMedicalEncounter;

public sealed class StartMedicalEncounterCommandHandler(
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IMedicalRecordRepository medicalRecordRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : IRequestHandler<StartMedicalEncounterCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        StartMedicalEncounterCommand request,
        CancellationToken cancellationToken
    )
    {
        var doctor =
            await doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.DoctorId
            );

        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        var appointmentType =
            await appointmentTypeRepository.GetByIdAsync(
                appointment.AppointmentTypeId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                appointment.AppointmentTypeId
            );

        appointment.Start(doctor.Id, timeProvider.GetUtcNow().UtcDateTime);

        var medicalRecord = MedicalEncounterService.InitiateMedicalRecord(
            appointment,
            request.ChiefComplaint,
            appointmentType.ProtectedCareCategory,
            request.GuardianInitiatedTreatment
        );

        await medicalRecordRepository.CreateAsync(medicalRecord, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return medicalRecord.Id;
    }
}

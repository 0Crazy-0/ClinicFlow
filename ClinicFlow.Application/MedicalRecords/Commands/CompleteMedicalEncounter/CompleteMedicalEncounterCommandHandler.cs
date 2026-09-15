using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;

public sealed class CompleteMedicalEncounterCommandHandler(
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IMedicalRecordRepository medicalRecordRepository,
    MedicalEncounterService medicalEncounterService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : IRequestHandler<CompleteMedicalEncounterCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        CompleteMedicalEncounterCommand request,
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

        var medicalRecord =
            await medicalRecordRepository.GetByIdAsync(request.MedicalRecordId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalRecord),
                request.MedicalRecordId
            );

        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = doctor,
            Appointment = appointment,
            AppointmentTypeDefinition = appointmentType,
            CompletedAt = timeProvider.GetUtcNow().UtcDateTime,
        };

        medicalEncounterService.ValidateAndCompleteRecord(medicalRecord, context);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return medicalRecord.Id;
    }
}

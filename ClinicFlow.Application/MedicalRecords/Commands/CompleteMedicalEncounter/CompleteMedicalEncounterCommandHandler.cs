using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
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
    public async Task<Guid> Handle(CompleteMedicalEncounterCommand request, CancellationToken ct)
    {
        var doctor =
            await doctorRepository.GetByIdAsync(request.DoctorId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Doctor),
                request.DoctorId
            );

        var appointment =
            await appointmentRepository.GetByIdAsync(request.AppointmentId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Appointment),
                request.AppointmentId
            );

        var appointmentType =
            await appointmentTypeRepository.GetByIdAsync(appointment.AppointmentTypeId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(AppointmentTypeDefinition),
                appointment.AppointmentTypeId
            );

        var details = request
            .Details.Select(dto =>
                DynamicClinicalDetail.Create(dto.TemplateCode, dto.JsonDataPayload)
            )
            .ToList();

        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = doctor,
            Appointment = appointment,
            AppointmentTypeDefinition = appointmentType,
            ProvidedDetails = details,
            CompletedAt = timeProvider.GetUtcNow().UtcDateTime,
        };

        var medicalRecord = MedicalEncounterService.InitiateMedicalRecord(
            appointment,
            request.ChiefComplaint
        );

        medicalEncounterService.ValidateAndCompleteRecord(medicalRecord, context);

        await medicalRecordRepository.CreateAsync(medicalRecord, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return medicalRecord.Id;
    }
}

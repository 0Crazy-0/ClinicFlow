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

public class CompleteMedicalEncounterCommandHandler(
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository,
    IMedicalRecordRepository medicalRecordRepository,
    MedicalEncounterService medicalEncounterService,
    IUnitOfWork unitOfWork
) : IRequestHandler<CompleteMedicalEncounterCommand, Guid>
{
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
        };

        var medicalRecord = MedicalRecord.Create(
            request.PatientId,
            request.DoctorId,
            request.AppointmentId,
            request.ChiefComplaint
        );

        medicalEncounterService.ValidateAndCompleteRecord(medicalRecord, context);

        await medicalRecordRepository.CreateAsync(medicalRecord, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return medicalRecord.Id;
    }
}

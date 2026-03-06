using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;

public class CompleteMedicalEncounterCommandHandler(IDoctorRepository doctorRepository, IAppointmentRepository appointmentRepository,
    IAppointmentTypeDefinitionRepository appointmentTypeRepository, IMedicalRecordRepository medicalRecordRepository,
    MedicalEncounterService medicalEncounterService, IUnitOfWork unitOfWork) : IRequestHandler<CompleteMedicalEncounterCommand, Guid>
{
    public async Task<Guid> Handle(CompleteMedicalEncounterCommand request, CancellationToken cancellationToken)
    {
        var doctor = await doctorRepository.GetByIdAsync(request.DoctorId) ?? throw new EntityNotFoundException(nameof(Doctor), request.DoctorId);

        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId) ?? throw new EntityNotFoundException(nameof(Appointment), request.AppointmentId);

        var appointmentType = await appointmentTypeRepository.GetByIdAsync(appointment.AppointmentTypeId) ??
            throw new EntityNotFoundException(nameof(AppointmentTypeDefinition), appointment.AppointmentTypeId);

        var details = request.Details.Select(dto => DynamicClinicalDetail.Create(dto.TemplateCode, dto.JsonDataPayload)).ToList();

        var context = new MedicalEncounterContext
        {
            ExpectedDoctor = doctor,
            Appointment = appointment,
            AppointmentTypeDefinition = appointmentType,
            ProvidedDetails = details
        };

        var medicalRecord = MedicalRecord.Create(request.PatientId, request.DoctorId, request.AppointmentId, request.ChiefComplaint);

        medicalEncounterService.ValidateAndCompleteRecord(medicalRecord, context);

        await medicalRecordRepository.CreateAsync(medicalRecord);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return medicalRecord.Id;
    }
}

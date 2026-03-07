using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordByAppointmentId;

public class GetMedicalRecordByAppointmentIdQueryHandler(IMedicalRecordRepository medicalRecordRepository) :
    IRequestHandler<GetMedicalRecordByAppointmentIdQuery, MedicalRecordDto>
{
    public async Task<MedicalRecordDto> Handle(GetMedicalRecordByAppointmentIdQuery request, CancellationToken cancellationToken)
    {
        var record = await medicalRecordRepository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(MedicalRecord), request.AppointmentId);

        return new MedicalRecordDto(record.Id, record.PatientId, record.DoctorId, record.AppointmentId, record.ChiefComplaint,
            record.ClinicalDetails.Select(d => new ClinicalDetailDto(d.TemplateCode, d.JsonDataPayload)));
    }
}

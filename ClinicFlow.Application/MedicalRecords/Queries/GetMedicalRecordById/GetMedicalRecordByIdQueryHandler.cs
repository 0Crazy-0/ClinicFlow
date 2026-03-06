using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordById;

public class GetMedicalRecordByIdQueryHandler(IMedicalRecordRepository medicalRecordRepository) : IRequestHandler<GetMedicalRecordByIdQuery, MedicalRecordDto>
{
    public async Task<MedicalRecordDto> Handle(GetMedicalRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await medicalRecordRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new EntityNotFoundException(nameof(MedicalRecord), request.Id);

        return new MedicalRecordDto(record.Id, record.PatientId, record.DoctorId, record.AppointmentId, record.ChiefComplaint,
            record.ClinicalDetails.Select(d => new ClinicalDetailDto(d.TemplateCode, d.JsonDataPayload)));
    }
}

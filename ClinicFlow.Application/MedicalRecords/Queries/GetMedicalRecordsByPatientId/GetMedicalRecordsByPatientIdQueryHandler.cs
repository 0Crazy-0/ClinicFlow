using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByPatientId;

public class GetMedicalRecordsByPatientIdQueryHandler(
    IMedicalRecordRepository medicalRecordRepository
) : IRequestHandler<GetMedicalRecordsByPatientIdQuery, IEnumerable<MedicalRecordDto>>
{
    public async Task<IEnumerable<MedicalRecordDto>> Handle(
        GetMedicalRecordsByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var records = await medicalRecordRepository.GetByPatientIdAsync(
            request.PatientId,
            cancellationToken
        );

        return records.Select(record => new MedicalRecordDto(
            record.Id,
            record.PatientId,
            record.DoctorId,
            record.AppointmentId,
            record.ChiefComplaint,
            record.ClinicalDetails.Select(d => new ClinicalDetailDto(
                d.TemplateCode,
                d.JsonDataPayload
            ))
        ));
    }
}

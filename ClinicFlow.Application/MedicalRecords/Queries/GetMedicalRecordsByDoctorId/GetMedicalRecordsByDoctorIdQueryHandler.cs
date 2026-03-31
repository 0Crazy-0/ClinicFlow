using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;

public sealed class GetMedicalRecordsByDoctorIdQueryHandler(
    IMedicalRecordRepository medicalRecordRepository
) : IRequestHandler<GetMedicalRecordsByDoctorIdQuery, IEnumerable<MedicalRecordDto>>
{
    public async Task<IEnumerable<MedicalRecordDto>> Handle(
        GetMedicalRecordsByDoctorIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var records = await medicalRecordRepository.GetByDoctorIdAsync(
            request.DoctorId,
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

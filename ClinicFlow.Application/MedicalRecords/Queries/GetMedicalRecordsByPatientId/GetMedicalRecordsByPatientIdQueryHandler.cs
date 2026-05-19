using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByPatientId;

public sealed class GetMedicalRecordsByPatientIdQueryHandler(
    IMedicalRecordRepository medicalRecordRepository
) : IRequestHandler<GetMedicalRecordsByPatientIdQuery, PaginatedList<MedicalRecordDto>>
{
    public async Task<PaginatedList<MedicalRecordDto>> Handle(
        GetMedicalRecordsByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var (items, totalCount) = await medicalRecordRepository.GetByPatientIdPaginatedAsync(
            request.PatientId,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var dtos = items
            .Select(record => new MedicalRecordDto(
                record.Id,
                record.PatientId,
                record.DoctorId,
                record.AppointmentId,
                record.ChiefComplaint,
                [
                    .. record.ClinicalDetails.Select(d => new ClinicalDetailDto(
                        d.TemplateCode,
                        d.JsonDataPayload
                    )),
                ]
            ))
            .ToList();

        return new PaginatedList<MedicalRecordDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}

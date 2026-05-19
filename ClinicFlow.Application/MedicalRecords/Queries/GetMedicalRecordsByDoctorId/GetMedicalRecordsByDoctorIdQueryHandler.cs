using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;

public sealed class GetMedicalRecordsByDoctorIdQueryHandler(
    IMedicalRecordRepository medicalRecordRepository
) : IRequestHandler<GetMedicalRecordsByDoctorIdQuery, PaginatedList<MedicalRecordDto>>
{
    public async Task<PaginatedList<MedicalRecordDto>> Handle(
        GetMedicalRecordsByDoctorIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var (items, totalCount) = await medicalRecordRepository.GetByDoctorIdPaginatedAsync(
            request.DoctorId,
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

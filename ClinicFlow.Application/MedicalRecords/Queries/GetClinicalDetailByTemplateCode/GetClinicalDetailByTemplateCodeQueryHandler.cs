using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetClinicalDetailByTemplateCode;

public sealed class GetClinicalDetailByTemplateCodeQueryHandler(
    IMedicalRecordRepository medicalRecordRepository
) : IRequestHandler<GetClinicalDetailByTemplateCodeQuery, ClinicalDetailDto?>
{
    public async Task<ClinicalDetailDto?> Handle(
        GetClinicalDetailByTemplateCodeQuery request,
        CancellationToken cancellationToken
    )
    {
        var record =
            await medicalRecordRepository.GetByIdAsync(request.MedicalRecordId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(MedicalRecord),
                request.MedicalRecordId
            );

        var detail = record.ClinicalDetails.FirstOrDefault(d =>
            d.TemplateCode == request.TemplateCode
        );

        return detail is null
            ? null
            : new ClinicalDetailDto(detail.TemplateCode, detail.JsonDataPayload);
    }
}

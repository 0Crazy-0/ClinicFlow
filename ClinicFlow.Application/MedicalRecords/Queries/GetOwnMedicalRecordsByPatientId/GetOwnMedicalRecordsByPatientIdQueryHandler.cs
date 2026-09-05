using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetOwnMedicalRecordsByPatientId;

public sealed class GetOwnMedicalRecordsByPatientIdQueryHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IMedicalRecordRepository medicalRecordRepository
) : IRequestHandler<GetOwnMedicalRecordsByPatientIdQuery, PaginatedList<MedicalRecordDto>>
{
    /// <inheritdoc />
    public async Task<PaginatedList<MedicalRecordDto>> Handle(
        GetOwnMedicalRecordsByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var membership = await familyMembershipRepository.GetActiveMembershipAsync(
            request.RequesterUserId,
            request.PatientId,
            cancellationToken
        );

        if (membership?.Role is not PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.MedicalRecord.UnauthorizedAccess);

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

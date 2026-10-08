using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services.Policies;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetFamilyMemberMedicalRecordsByPatientId;

public sealed class GetFamilyMemberMedicalRecordsByPatientIdQueryHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IPatientRepository patientRepository,
    IMedicalRecordRepository medicalRecordRepository,
    IMedicalRecordConsentGrantRepository consentGrantRepository,
    TimeProvider timeProvider
) : IRequestHandler<GetFamilyMemberMedicalRecordsByPatientIdQuery, PaginatedList<MedicalRecordDto>>
{
    /// <inheritdoc />
    public async Task<PaginatedList<MedicalRecordDto>> Handle(
        GetFamilyMemberMedicalRecordsByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var membership =
            await familyMembershipRepository.GetActiveMembershipAsync(
                request.RequesterUserId,
                request.PatientId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(FamilyMembership),
                request.PatientId
            );

        membership.EnsureMedicalRecordsAccess();

        var patient =
            await patientRepository.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(Patient),
                request.PatientId
            );

        var patientAge = patient.GetAge(DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
        var excludedCategories = ProtectedCategoryPolicy.GetProtectedCategoriesFor(patientAge);

        IReadOnlyCollection<Guid> authorizedRecordIds = [];

        // Adults have no excluded categories, so grants cannot affect visibility. Skip the query.
        if (patientAge < DomainRules.AdultAge)
        {
            authorizedRecordIds = await consentGrantRepository.GetEffectiveRecordIdsAsync(
                request.PatientId,
                membership.Id,
                timeProvider.GetLocalNow().DateTime,
                cancellationToken
            );
        }

        var (items, totalCount) =
            await medicalRecordRepository.GetByPatientIdPaginatedExcludingCategoriesAsync(
                request.PatientId,
                excludedCategories,
                authorizedRecordIds,
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

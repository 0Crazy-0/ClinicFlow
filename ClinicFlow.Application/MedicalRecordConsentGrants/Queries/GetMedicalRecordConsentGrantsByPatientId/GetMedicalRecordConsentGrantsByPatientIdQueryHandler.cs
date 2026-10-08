using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.MedicalRecordConsentGrants.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.MedicalRecordConsentGrants.Queries.GetMedicalRecordConsentGrantsByPatientId;

public sealed class GetMedicalRecordConsentGrantsByPatientIdQueryHandler(
    IFamilyMembershipRepository familyMembershipRepository,
    IMedicalRecordConsentGrantRepository consentGrantRepository,
    TimeProvider timeProvider
)
    : IRequestHandler<
        GetMedicalRecordConsentGrantsByPatientIdQuery,
        PaginatedList<MedicalRecordConsentGrantDto>
    >
{
    /// <inheritdoc />
    public async Task<PaginatedList<MedicalRecordConsentGrantDto>> Handle(
        GetMedicalRecordConsentGrantsByPatientIdQuery request,
        CancellationToken cancellationToken
    )
    {
        if (
            !await familyMembershipRepository.HasActiveSelfMembershipAsync(
                request.RequesterUserId,
                request.PatientId,
                cancellationToken
            )
        )
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.UnauthorizedAccess
            );

        var (items, totalCount) = await consentGrantRepository.GetByPatientIdPaginatedAsync(
            request.PatientId,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var referenceTime = timeProvider.GetLocalNow().DateTime;

        var dtos = items
            .Select(grant => new MedicalRecordConsentGrantDto(
                grant.Id,
                grant.PatientId,
                grant.RecipientMembershipId,
                grant.MedicalRecordId,
                grant.SignedByUserId,
                grant.SignedAt,
                grant.ExpiresAt,
                grant.RevokedAt,
                grant.IsEffectiveAt(referenceTime)
            ))
            .ToList();

        return new PaginatedList<MedicalRecordConsentGrantDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}

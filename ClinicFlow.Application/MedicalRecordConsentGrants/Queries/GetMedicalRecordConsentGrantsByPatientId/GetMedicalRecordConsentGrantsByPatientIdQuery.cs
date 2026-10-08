using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.MedicalRecordConsentGrants.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecordConsentGrants.Queries.GetMedicalRecordConsentGrantsByPatientId;

public sealed record GetMedicalRecordConsentGrantsByPatientIdQuery(
    Guid RequesterUserId,
    Guid PatientId,
    int PageNumber,
    int PageSize
) : IRequest<PaginatedList<MedicalRecordConsentGrantDto>>;

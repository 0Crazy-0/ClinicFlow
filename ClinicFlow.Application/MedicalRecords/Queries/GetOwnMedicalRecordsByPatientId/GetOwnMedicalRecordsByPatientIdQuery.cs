using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetOwnMedicalRecordsByPatientId;

public sealed record GetOwnMedicalRecordsByPatientIdQuery(
    Guid RequesterUserId,
    Guid PatientId,
    int PageNumber,
    int PageSize
) : IRequest<PaginatedList<MedicalRecordDto>>;

using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByPatientId;

public sealed record GetMedicalRecordsByPatientIdQuery(Guid PatientId, int PageNumber, int PageSize)
    : IRequest<PaginatedList<MedicalRecordDto>>;

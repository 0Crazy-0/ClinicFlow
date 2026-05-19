using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;

public sealed record GetMedicalRecordsByDoctorIdQuery(Guid DoctorId, int PageNumber, int PageSize)
    : IRequest<PaginatedList<MedicalRecordDto>>;

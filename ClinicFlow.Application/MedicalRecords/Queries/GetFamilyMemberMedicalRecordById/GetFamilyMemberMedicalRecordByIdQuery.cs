using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetFamilyMemberMedicalRecordById;

public sealed record GetFamilyMemberMedicalRecordByIdQuery(
    Guid RequesterUserId,
    Guid MedicalRecordId
) : IRequest<MedicalRecordDto>;

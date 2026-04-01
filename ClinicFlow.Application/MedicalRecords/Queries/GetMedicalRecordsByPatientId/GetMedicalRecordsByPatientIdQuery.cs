using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByPatientId;

public sealed record GetMedicalRecordsByPatientIdQuery(Guid PatientId)
    : IRequest<IReadOnlyList<MedicalRecordDto>>;

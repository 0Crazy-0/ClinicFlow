using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;

public sealed record GetMedicalRecordsByDoctorIdQuery(Guid DoctorId)
    : IRequest<IEnumerable<MedicalRecordDto>>;

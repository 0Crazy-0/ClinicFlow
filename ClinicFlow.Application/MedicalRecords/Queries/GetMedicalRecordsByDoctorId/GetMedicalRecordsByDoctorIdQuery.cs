using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;

public record GetMedicalRecordsByDoctorIdQuery(Guid DoctorId)
    : IRequest<IEnumerable<MedicalRecordDto>>;

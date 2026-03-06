using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByPatientId;

public record GetMedicalRecordsByPatientIdQuery(Guid PatientId) : IRequest<IEnumerable<MedicalRecordDto>>;

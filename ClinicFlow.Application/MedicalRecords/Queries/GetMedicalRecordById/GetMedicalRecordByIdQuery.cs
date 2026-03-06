using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordById;

public record GetMedicalRecordByIdQuery(Guid Id) : IRequest<MedicalRecordDto>;

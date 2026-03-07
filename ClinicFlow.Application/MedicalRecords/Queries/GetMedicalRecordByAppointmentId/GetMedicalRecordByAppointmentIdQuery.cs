using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordByAppointmentId;

public record GetMedicalRecordByAppointmentIdQuery(Guid AppointmentId) : IRequest<MedicalRecordDto>;

using ClinicFlow.Application.MedicalRecords.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordByAppointmentId;

public sealed record GetMedicalRecordByAppointmentIdQuery(Guid AppointmentId)
    : IRequest<MedicalRecordDto>;

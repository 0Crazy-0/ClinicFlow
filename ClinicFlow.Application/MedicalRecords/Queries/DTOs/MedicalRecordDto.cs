namespace ClinicFlow.Application.MedicalRecords.Queries.DTOs;

public sealed record MedicalRecordDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    Guid AppointmentId,
    string ChiefComplaint,
    IReadOnlyList<ClinicalDetailDto> ClinicalDetails
);

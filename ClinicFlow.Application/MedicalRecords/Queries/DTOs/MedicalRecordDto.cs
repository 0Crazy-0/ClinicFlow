namespace ClinicFlow.Application.MedicalRecords.Queries.DTOs;

public record MedicalRecordDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    Guid AppointmentId,
    string ChiefComplaint,
    IEnumerable<ClinicalDetailDto> ClinicalDetails
);

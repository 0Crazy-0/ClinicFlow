using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Appointments.Queries.DTOs;

public sealed record AppointmentDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    Guid AppointmentTypeId,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    AppointmentStatus Status,
    string PatientNotes,
    string ReceptionistNotes
);

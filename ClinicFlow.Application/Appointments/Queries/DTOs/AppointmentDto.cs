using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Appointments.Queries.DTOs;

public sealed record AppointmentDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    Guid AppointmentTypeId,
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    AppointmentStatus Status,
    string PatientNotes,
    string ReceptionistNotes
);

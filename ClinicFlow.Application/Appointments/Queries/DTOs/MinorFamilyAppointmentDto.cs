using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.Appointments.Queries.DTOs;

public sealed record MinorFamilyAppointmentDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    Guid AppointmentTypeId,
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    AppointmentStatus Status,
    string ReceptionistNotes,
    string GuardianNotes
);

using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByDoctor;

public sealed record RescheduleByDoctorCommand(
    Guid InitiatorUserId,
    Guid AppointmentId,
    DateOnly NewDate,
    TimeOnly NewStartTime,
    TimeOnly NewEndTime,
    bool IsOverbook
) : IRequest;

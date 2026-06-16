using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByStaff;

public sealed record RescheduleByStaffCommand(
    Guid InitiatorUserId,
    Guid AppointmentId,
    DateOnly NewDate,
    TimeOnly NewStartTime,
    TimeOnly NewEndTime,
    bool IsOverbook
) : IRequest;

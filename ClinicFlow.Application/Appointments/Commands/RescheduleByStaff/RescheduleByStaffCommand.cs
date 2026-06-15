using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByStaff;

public sealed record RescheduleByStaffCommand(
    Guid InitiatorUserId,
    Guid AppointmentId,
    DateTime NewDate,
    TimeOnly NewStartTime,
    TimeOnly NewEndTime,
    bool IsOverbook
) : IRequest;

using ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByStaff;

public sealed record RescheduleByStaffCommand(
    Guid InitiatorUserId,
    Guid AppointmentId,
    DateTime NewDate,
    TimeSpan NewStartTime,
    TimeSpan NewEndTime,
    bool IsOverbook
) : IRequest, IRescheduleCommand;

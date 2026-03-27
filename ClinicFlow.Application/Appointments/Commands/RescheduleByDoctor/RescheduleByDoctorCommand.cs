using ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByDoctor;

public record RescheduleByDoctorCommand(
    Guid InitiatorUserId,
    Guid AppointmentId,
    DateTime NewDate,
    TimeSpan NewStartTime,
    TimeSpan NewEndTime,
    bool IsOverbook
) : IRequest, IRescheduleCommand;

using ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;

public record RescheduleByPatientCommand(
    Guid InitiatorUserId,
    Guid AppointmentId,
    DateTime NewDate,
    TimeSpan NewStartTime,
    TimeSpan NewEndTime
) : IRequest, IRescheduleCommand;

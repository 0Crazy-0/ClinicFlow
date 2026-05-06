using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ReassignAppointment;

public sealed record ReassignAppointmentCommand(
    Guid AppointmentId,
    Guid NewDoctorId,
    DateTime NewDate,
    TimeSpan NewStartTime,
    TimeSpan NewEndTime
) : IRequest;

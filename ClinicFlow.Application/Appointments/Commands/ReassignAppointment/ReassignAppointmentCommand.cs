using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ReassignAppointment;

public sealed record ReassignAppointmentCommand(
    Guid AppointmentId,
    Guid NewDoctorId,
    DateOnly NewDate,
    TimeOnly NewStartTime,
    TimeOnly NewEndTime
) : IRequest;

using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByStaff;

public record MarkAppointmentAsNoShowByStaffCommand(Guid AppointmentId, Guid InitiatorUserId)
    : IRequest;

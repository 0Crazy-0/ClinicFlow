using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByStaff;

public sealed record MarkAppointmentAsNoShowByStaffCommand(Guid AppointmentId) : IRequest;

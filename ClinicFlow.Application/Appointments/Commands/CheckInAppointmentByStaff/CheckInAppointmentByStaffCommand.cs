using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.CheckInAppointmentByStaff;

public sealed record CheckInAppointmentByStaffCommand(
    Guid AppointmentId,
    string? ReceptionistNotes = null
) : IRequest;

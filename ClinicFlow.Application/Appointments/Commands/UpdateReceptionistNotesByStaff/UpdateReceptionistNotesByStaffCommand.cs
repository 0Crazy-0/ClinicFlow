using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.UpdateReceptionistNotesByStaff;

public sealed record UpdateReceptionistNotesByStaffCommand(Guid AppointmentId, string? Notes)
    : IRequest;

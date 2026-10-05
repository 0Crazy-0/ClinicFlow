using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.UpdateStaffInternalNotesByStaff;

public sealed record UpdateStaffInternalNotesByStaffCommand(Guid AppointmentId, string? Notes)
    : IRequest;

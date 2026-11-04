using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ClearStaffInternalNotesByStaff;

public sealed record ClearStaffInternalNotesByStaffCommand(Guid AppointmentId) : IRequest;


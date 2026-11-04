using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ClearReceptionistNotesByStaff;

public sealed record ClearReceptionistNotesByStaffCommand(Guid AppointmentId) : IRequest;

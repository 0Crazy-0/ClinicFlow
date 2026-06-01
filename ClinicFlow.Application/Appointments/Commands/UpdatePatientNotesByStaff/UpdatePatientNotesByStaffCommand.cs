using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.UpdatePatientNotesByStaff;

public sealed record UpdatePatientNotesByStaffCommand(Guid AppointmentId, string? Notes) : IRequest;

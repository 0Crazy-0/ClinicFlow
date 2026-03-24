using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByDoctor;

public record MarkAppointmentAsNoShowByDoctorCommand(Guid AppointmentId, Guid InitiatorUserId)
    : IRequest;

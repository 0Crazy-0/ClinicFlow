using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.StartAppointmentByDoctor;

public sealed record StartAppointmentByDoctorCommand(Guid AppointmentId, Guid InitiatorUserId)
    : IRequest;

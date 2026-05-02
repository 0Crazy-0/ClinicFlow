using MediatR;

namespace ClinicFlow.Application.Patients.Commands.ClosePatientAccount;

public sealed record ClosePatientAccountCommand(Guid UserId) : IRequest;

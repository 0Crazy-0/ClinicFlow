using MediatR;

namespace ClinicFlow.Application.Patients.Commands.LeaveFamilyAccount;

public sealed record LeaveFamilyAccountCommand(Guid PatientId, Guid InitiatorUserId) : IRequest;

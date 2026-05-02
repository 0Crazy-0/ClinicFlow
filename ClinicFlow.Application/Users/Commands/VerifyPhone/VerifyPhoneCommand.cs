using MediatR;

namespace ClinicFlow.Application.Users.Commands.VerifyPhone;

public sealed record VerifyPhoneCommand(Guid UserId, string Code) : IRequest;

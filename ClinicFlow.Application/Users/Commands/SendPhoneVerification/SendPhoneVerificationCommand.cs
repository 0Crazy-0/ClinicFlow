using MediatR;

namespace ClinicFlow.Application.Users.Commands.SendPhoneVerification;

public sealed record SendPhoneVerificationCommand(Guid UserId) : IRequest;

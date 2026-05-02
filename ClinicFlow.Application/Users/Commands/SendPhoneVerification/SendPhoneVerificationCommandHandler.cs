using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.SendPhoneVerification;

public sealed class SendPhoneVerificationCommandHandler(
    IUserRepository userRepository,
    IPhoneVerificationService phoneVerificationService
) : IRequestHandler<SendPhoneVerificationCommand>
{
    public async Task Handle(
        SendPhoneVerificationCommand request,
        CancellationToken cancellationToken
    )
    {
        var user =
            await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
                request.UserId
            );

        await phoneVerificationService.SendVerificationCodeAsync(
            user.PhoneNumber,
            cancellationToken
        );
    }
}

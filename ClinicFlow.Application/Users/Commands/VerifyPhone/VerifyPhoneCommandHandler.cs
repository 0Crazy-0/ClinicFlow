using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.VerifyPhone;

public sealed class VerifyPhoneCommandHandler(
    IUserRepository userRepository,
    IPhoneVerificationService phoneVerificationService,
    IUnitOfWork unitOfWork
) : IRequestHandler<VerifyPhoneCommand>
{
    public async Task Handle(VerifyPhoneCommand request, CancellationToken ct)
    {
        var user =
            await userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
                request.UserId
            );

        var isValid = await phoneVerificationService.VerifyCodeAsync(
            user.PhoneNumber,
            request.Code,
            ct
        );

        user.MarkPhoneAsVerified(isValid);

        await unitOfWork.SaveChangesAsync(ct);
    }
}

using ClinicFlow.Application.Interfaces;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IPasswordResetTokenService passwordResetTokenService,
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService,
    IUnitOfWork unitOfWork
) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId =
            await passwordResetTokenService.ValidateTokenAsync(request.Token, cancellationToken)
            ?? throw new DomainValidationException(DomainErrors.Validation.InvalidValue);

        var user =
            await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
                userId
            );

        var newHash = passwordHasherService.Hash(request.NewPassword);

        user.ChangePassword(newHash);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

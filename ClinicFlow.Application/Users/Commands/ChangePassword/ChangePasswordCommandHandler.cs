using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService,
    IUnitOfWork unitOfWork
) : IRequestHandler<ChangePasswordCommand>
{
    /// <inheritdoc />
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user =
            await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
                request.UserId
            );

        var isCurrentValid = passwordHasherService.Verify(
            request.CurrentPassword,
            user.PasswordHash
        );

        if (!isCurrentValid)
            throw new BusinessRuleValidationException(DomainErrors.User.InvalidCredentials);

        var newHash = passwordHasherService.Hash(request.NewPassword);

        user.ChangePassword(newHash);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

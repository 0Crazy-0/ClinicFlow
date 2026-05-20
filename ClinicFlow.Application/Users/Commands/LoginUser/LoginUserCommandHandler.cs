using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.Services;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.LoginUser;

public sealed class LoginUserCommandHandler(
    TimeProvider timeProvider,
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService,
    IUnitOfWork unitOfWork
) : IRequestHandler<LoginUserCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user =
            await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new BusinessRuleValidationException(DomainErrors.User.InvalidCredentials);

        var isValid = passwordHasherService.Verify(request.Password, user.PasswordHash);

        var authenticated = UserAuthenticationService.TryAuthenticate(
            user,
            isValid,
            timeProvider.GetUtcNow().UtcDateTime
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (!authenticated)
            throw new BusinessRuleValidationException(DomainErrors.User.InvalidCredentials);

        return user.Id;
    }
}

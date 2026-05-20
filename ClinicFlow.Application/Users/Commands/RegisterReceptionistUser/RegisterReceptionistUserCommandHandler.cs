using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.RegisterReceptionistUser;

public sealed class RegisterReceptionistUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService,
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterReceptionistUserCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(
        RegisterReceptionistUserCommand request,
        CancellationToken cancellationToken
    )
    {
        if (await userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new BusinessRuleValidationException(DomainErrors.User.EmailAlreadyExists);

        var email = EmailAddress.Create(request.Email);
        var phone = PhoneNumber.Create(request.PhoneNumber);
        var passwordHash = passwordHasherService.Hash(request.Password);
        var user = User.Create(email, passwordHash, phone, UserRole.Receptionist);

        await userRepository.CreateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}

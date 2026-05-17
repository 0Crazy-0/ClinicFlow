using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.ValueObjects;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService,
    IUnitOfWork unitOfWork
) : IRequestHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new BusinessRuleValidationException(DomainErrors.User.EmailAlreadyExists);

        var email = EmailAddress.Create(request.Email);
        var phone = PhoneNumber.Create(request.PhoneNumber);
        var passwordHash = passwordHasherService.Hash(request.Password);

        var user = User.Create(email, passwordHash, phone, UserRole.Patient);

        await userRepository.CreateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}

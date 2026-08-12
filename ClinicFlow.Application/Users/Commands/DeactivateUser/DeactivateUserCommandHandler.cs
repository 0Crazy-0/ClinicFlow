using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler(
    IUserRepository userRepository,
    IFamilyMembershipRepository familyMembershipRepository,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeactivateUserCommand>
{
    /// <inheritdoc />
    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user =
            await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
                request.UserId
            );

        var selfMembership =
            await familyMembershipRepository.GetActiveSelfMembershipByUserIdAsync(
                request.UserId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(FamilyMembership),
                request.UserId
            );

        if (
            await familyMembershipRepository.CountActiveFamilyMembersAsync(
                request.UserId,
                cancellationToken
            ) > 0
        )
            throw new DomainValidationException(
                DomainErrors.User.CannotCloseAccountWithActiveFamilyMembers
            );

        user.Deactivate();
        selfMembership.Leave(timeProvider.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

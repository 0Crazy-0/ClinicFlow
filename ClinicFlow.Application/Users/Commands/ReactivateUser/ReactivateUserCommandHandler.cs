using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Users.Commands.ReactivateUser;

public sealed class ReactivateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReactivateUserCommand>
{
    /// <inheritdoc />
    public async Task Handle(ReactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user =
            await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
                request.UserId
            );

        user.Reactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.CheckEmailUniqueness;

public sealed class CheckEmailUniquenessQueryHandler(IUserRepository userRepository)
    : IRequestHandler<CheckEmailUniquenessQuery, bool>
{
    public async Task<bool> Handle(
        CheckEmailUniquenessQuery request,
        CancellationToken cancellationToken
    )
    {
        var exists = await userRepository.ExistsByEmailAsync(request.Email, cancellationToken);

        return !exists;
    }
}

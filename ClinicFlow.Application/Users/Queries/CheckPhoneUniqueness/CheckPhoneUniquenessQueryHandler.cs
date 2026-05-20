using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.CheckPhoneUniqueness;

public sealed class CheckPhoneUniquenessQueryHandler(IUserRepository userRepository)
    : IRequestHandler<CheckPhoneUniquenessQuery, bool>
{
    /// <inheritdoc />
    public async Task<bool> Handle(
        CheckPhoneUniquenessQuery request,
        CancellationToken cancellationToken
    )
    {
        var exists = await userRepository.ExistsByPhoneNumberAsync(
            request.PhoneNumber,
            cancellationToken
        );

        return !exists;
    }
}

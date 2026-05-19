using ClinicFlow.Application.Users.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.GetLockedOutUsers;

public sealed class GetLockedOutUsersQueryHandler(
    TimeProvider timeProvider,
    IUserRepository userRepository
) : IRequestHandler<GetLockedOutUsersQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> Handle(
        GetLockedOutUsersQuery request,
        CancellationToken cancellationToken
    )
    {
        var users = await userRepository.GetLockedOutUsersAsync(
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken
        );

        return
        [
            .. users.Select(user => new UserDto(
                user.Id,
                user.Email.Value,
                user.PhoneNumber.Value,
                user.Role,
                user.IsActive,
                user.IsPhoneVerified,
                user.LastLoginAt,
                user.FailedLoginAttempts,
                user.LockoutEnd
            )),
        ];
    }
}

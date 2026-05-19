using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Users.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.GetLockedOutUsers;

public sealed class GetLockedOutUsersQueryHandler(
    TimeProvider timeProvider,
    IUserRepository userRepository
) : IRequestHandler<GetLockedOutUsersQuery, PaginatedList<UserDto>>
{
    public async Task<PaginatedList<UserDto>> Handle(
        GetLockedOutUsersQuery request,
        CancellationToken cancellationToken
    )
    {
        var (items, totalCount) = await userRepository.GetLockedOutUsersPaginatedAsync(
            timeProvider.GetUtcNow().UtcDateTime,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var dtos = items
            .Select(user => new UserDto(
                user.Id,
                user.Email.Value,
                user.PhoneNumber.Value,
                user.Role,
                user.IsActive,
                user.IsPhoneVerified,
                user.LastLoginAt,
                user.FailedLoginAttempts,
                user.LockoutEnd
            ))
            .ToList();

        return new PaginatedList<UserDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}

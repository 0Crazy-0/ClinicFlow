using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Users.Queries.DTOs;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
{
    /// <inheritdoc />
    public async Task<PaginatedList<UserDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken
    )
    {
        var (items, totalCount) = await userRepository.GetPaginatedAsync(
            request.PageNumber,
            request.PageSize,
            request.Role,
            request.IsActive,
            request.SearchTerm,
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

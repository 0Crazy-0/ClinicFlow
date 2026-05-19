using ClinicFlow.Application.Users.Queries.DTOs;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user =
            await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new EntityNotFoundException(
                DomainErrors.General.NotFound,
                nameof(User),
                request.UserId
            );

        return new UserDto(
            user.Id,
            user.Email.Value,
            user.PhoneNumber.Value,
            user.Role,
            user.IsActive,
            user.IsPhoneVerified,
            user.LastLoginAt,
            user.FailedLoginAttempts,
            user.LockoutEnd
        );
    }
}

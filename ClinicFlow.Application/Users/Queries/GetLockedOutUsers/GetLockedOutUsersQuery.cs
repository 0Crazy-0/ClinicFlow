using ClinicFlow.Application.Users.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.GetLockedOutUsers;

public sealed record GetLockedOutUsersQuery() : IRequest<IReadOnlyList<UserDto>>;

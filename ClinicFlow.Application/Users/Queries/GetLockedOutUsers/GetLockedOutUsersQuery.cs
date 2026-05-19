using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Users.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.GetLockedOutUsers;

public sealed record GetLockedOutUsersQuery(int PageNumber, int PageSize)
    : IRequest<PaginatedList<UserDto>>;

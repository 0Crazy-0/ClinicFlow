using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Users.Queries.DTOs;
using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.GetPaginatedUsers;

public sealed record GetPaginatedUsersQuery(
    int PageNumber,
    int PageSize,
    UserRole? Role,
    bool? IsActive,
    string? SearchTerm
) : IRequest<PaginatedList<UserDto>>;

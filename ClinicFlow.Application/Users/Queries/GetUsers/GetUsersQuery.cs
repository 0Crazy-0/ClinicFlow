using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Users.Queries.DTOs;
using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    int PageNumber,
    int PageSize,
    UserRole? Role,
    bool? IsActive,
    string? SearchTerm
) : IRequest<PaginatedList<UserDto>>;

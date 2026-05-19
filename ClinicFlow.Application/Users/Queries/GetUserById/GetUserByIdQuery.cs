using ClinicFlow.Application.Users.Queries.DTOs;
using MediatR;

namespace ClinicFlow.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;

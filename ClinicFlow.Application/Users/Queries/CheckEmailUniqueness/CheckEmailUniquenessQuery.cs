using MediatR;

namespace ClinicFlow.Application.Users.Queries.CheckEmailUniqueness;

public sealed record CheckEmailUniquenessQuery(string Email) : IRequest<bool>;

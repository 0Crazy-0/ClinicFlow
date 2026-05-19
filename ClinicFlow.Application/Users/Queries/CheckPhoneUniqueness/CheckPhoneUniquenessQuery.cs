using MediatR;

namespace ClinicFlow.Application.Users.Queries.CheckPhoneUniqueness;

public sealed record CheckPhoneUniquenessQuery(string PhoneNumber) : IRequest<bool>;

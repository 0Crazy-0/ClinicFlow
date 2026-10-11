using ClinicFlow.Domain.Enums;
using MediatR;

namespace ClinicFlow.Application.AppointmentConsentGrants.Commands.GrantAppointmentCategoryConsent;

public sealed record GrantAppointmentCategoryConsentCommand(
    Guid RequesterUserId,
    Guid PatientId,
    Guid RecipientMembershipId,
    ProtectedCategory ProtectedCareCategory
) : IRequest<Guid>;

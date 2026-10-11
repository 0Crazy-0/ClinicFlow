using MediatR;

namespace ClinicFlow.Application.AppointmentConsentGrants.Commands.GrantSpecificAppointmentConsent;

public sealed record GrantSpecificAppointmentConsentCommand(
    Guid RequesterUserId,
    Guid PatientId,
    Guid RecipientMembershipId,
    Guid AppointmentId
) : IRequest<Guid>;

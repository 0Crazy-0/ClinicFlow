using MediatR;

namespace ClinicFlow.Application.AppointmentConsentGrants.Commands.RevokeAppointmentConsentGrant;

public sealed record RevokeAppointmentConsentGrantCommand(Guid RequesterUserId, Guid ConsentGrantId)
    : IRequest;

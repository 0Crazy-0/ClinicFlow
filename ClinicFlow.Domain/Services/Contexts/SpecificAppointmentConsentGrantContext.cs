using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record SpecificAppointmentConsentGrantContext
{
    public required Patient Patient { get; init; }
    public required FamilyMembership RequesterMembership { get; init; }
    public required FamilyMembership RecipientMembership { get; init; }
    public required Appointment Appointment { get; init; }
    public required AppointmentTypeDefinition AppointmentType { get; init; }
    public bool HasEffectiveGrant { get; init; }
}

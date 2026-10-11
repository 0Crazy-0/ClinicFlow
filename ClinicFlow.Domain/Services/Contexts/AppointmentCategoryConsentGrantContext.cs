using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record AppointmentCategoryConsentGrantContext
{
    public required Patient Patient { get; init; }
    public required FamilyMembership RequesterMembership { get; init; }
    public required FamilyMembership RecipientMembership { get; init; }
    public required ProtectedCategory ProtectedCareCategory { get; init; }
    public bool HasEffectiveGrant { get; init; }
}

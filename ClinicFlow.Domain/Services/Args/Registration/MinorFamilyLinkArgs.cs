using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Args.Registration;

/// <summary>
/// Holds the guardian's intent when linking a minor patient to their account.
/// </summary>
public sealed record MinorFamilyLinkArgs
{
    public PatientRelationship Relationship { get; init; }
    public LegalAuthorityType LegalAuthority { get; init; }
}

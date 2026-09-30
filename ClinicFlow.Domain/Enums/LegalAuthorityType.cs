namespace ClinicFlow.Domain.Enums;

/// <summary>
/// Defines the legal authority a family member holds over the linked patient profile.
/// </summary>
/// <remarks>
/// Role describes the family bond and the authority value the legal fact. This allows a parent without
/// authority (loss of custody, for example) and a guardian who is not a relative. The business
/// question is always answered with a single field: the authority is not None while the patient
/// is a minor, which is the idiomatic form.
/// Parent requires Role Parent. Guardian requires Role Other or Sibling. None is coherent
/// with any role. A minor patient always requires an authority other than None, and an
/// authority other than None requires a minor patient. Self memberships always use None.
/// An authority other than None is currently rejected for adult patients. Adult conservatorship
/// under Cal. Prob. Code sections 1800 to 1804 requires a dedicated design and is not modeled yet,
/// so this restriction is a current scope limitation and must not be read as permanent.
/// </remarks>
public enum LegalAuthorityType
{
    None = 0,
    Parent = 1,
    Guardian = 2,
}

using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Records a minor patient's written authorization allowing a specific family member
/// to see protected appointments, either of a whole protected care category or of
/// a specific appointment.
/// </summary>
/// <remarks>
/// Only the minor patient may sign, through their active Self membership. The legal
/// representative cannot sign authorizations over services the minor could consent to
/// under Family Code, Division 11, Part 4, as required by
/// <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=CIV&sectionNum=56.11">Cal. Civ. Code § 56.11(b)(3)(A)</see>
/// and
/// <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=CIV&sectionNum=56.11">§ 56.11(b)(3)(B)</see>.
/// This grant is consent evidence and is never deleted. Revocation takes effect at the
/// instant it is recorded, following
/// <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=CIV&sectionNum=56.15">Cal. Civ. Code § 56.15</see>.
/// It is not retroactive and no notice is sent to the family member.
/// </remarks>
public class AppointmentConsentGrant : BaseEntity
{
    /// <summary>
    /// Validity duration in years of a signed authorization.
    /// </summary>
    /// <remarks>
    /// Fixed by <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=CIV&sectionNum=56.11">Cal. Civ. Code § 56.11(b)(8)</see>,
    /// which caps authorization validity at one year. Centralized here as a single
    /// constant so it can become configurable later without touching creation logic.
    /// </remarks>
    public const int ValidityYears = 1;

    public Guid PatientId { get; private set; }

    public Guid RecipientMembershipId { get; private set; }

    public AppointmentConsentScope Scope { get; private set; }

    /// <summary>
    /// The authorized protected care category. Set only for category scope grants.
    /// </summary>
    public ProtectedCategory? ProtectedCareCategory { get; private set; }

    /// <summary>
    /// The authorized appointment. Set only for specific appointment scope grants.
    /// </summary>
    public Guid? AppointmentId { get; private set; }

    public Guid SignedByUserId { get; private set; }

    public DateTime SignedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    /// <remarks>
    /// Null while the grant is in force. Populated when the patient revokes it.
    /// </remarks>
    public DateTime? RevokedAt { get; private set; }

    private AppointmentConsentGrant() { }

    /// <summary>
    /// Creates a category scope grant covering every appointment of the given protected category.
    /// </summary>
    private AppointmentConsentGrant(
        Guid patientId,
        Guid recipientMembershipId,
        ProtectedCategory protectedCareCategory,
        Guid signedByUserId,
        DateTime signedAt
    )
        : this()
    {
        PatientId = patientId;
        RecipientMembershipId = recipientMembershipId;
        Scope = AppointmentConsentScope.Category;
        ProtectedCareCategory = protectedCareCategory;
        SignedByUserId = signedByUserId;
        SignedAt = signedAt;
        ExpiresAt = signedAt.AddYears(ValidityYears);
    }

    /// <summary>
    /// Creates a specific appointment scope grant covering a single appointment.
    /// </summary>
    private AppointmentConsentGrant(
        Guid patientId,
        Guid recipientMembershipId,
        Guid appointmentId,
        Guid signedByUserId,
        DateTime signedAt
    )
        : this()
    {
        PatientId = patientId;
        RecipientMembershipId = recipientMembershipId;
        Scope = AppointmentConsentScope.SpecificAppointment;
        AppointmentId = appointmentId;
        SignedByUserId = signedByUserId;
        SignedAt = signedAt;
        ExpiresAt = signedAt.AddYears(ValidityYears);
    }

    internal static AppointmentConsentGrant CreateForCategory(
        Guid patientId,
        Guid recipientMembershipId,
        ProtectedCategory protectedCareCategory,
        Guid signedByUserId,
        DateTime signedAt
    )
    {
        Guard.NotEmpty(patientId);
        Guard.NotEmpty(recipientMembershipId);
        Guard.IsDefined(protectedCareCategory);
        Guard.NotEmpty(signedByUserId);
        Guard.NotDefault(signedAt);

        return new AppointmentConsentGrant(
            patientId,
            recipientMembershipId,
            protectedCareCategory,
            signedByUserId,
            signedAt
        );
    }

    internal static AppointmentConsentGrant CreateForAppointment(
        Guid patientId,
        Guid recipientMembershipId,
        Guid appointmentId,
        Guid signedByUserId,
        DateTime signedAt
    )
    {
        Guard.NotEmpty(patientId);
        Guard.NotEmpty(recipientMembershipId);
        Guard.NotEmpty(appointmentId);
        Guard.NotEmpty(signedByUserId);
        Guard.NotDefault(signedAt);

        return new AppointmentConsentGrant(
            patientId,
            recipientMembershipId,
            appointmentId,
            signedByUserId,
            signedAt
        );
    }

    public bool IsEffectiveAt(DateTime referenceTime)
    {
        Guard.NotDefault(referenceTime);

        return RevokedAt is null && referenceTime < ExpiresAt;
    }

    public void Revoke(bool requesterIsAuthorized, DateTime referenceTime)
    {
        Guard.NotDefault(referenceTime);

        if (!requesterIsAuthorized)
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.UnauthorizedRevocation
            );

        if (RevokedAt is not null)
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.AlreadyRevoked
            );

        if (referenceTime >= ExpiresAt)
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.AlreadyExpired
            );

        if (referenceTime <= SignedAt)
            throw new DomainValidationException(
                DomainErrors.Validation.EndTimeMustBeAfterStartTime
            );

        RevokedAt = referenceTime;
    }
}

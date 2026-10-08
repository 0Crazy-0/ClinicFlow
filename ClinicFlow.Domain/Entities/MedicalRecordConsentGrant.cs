using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Records a minor patient's written authorization allowing a specific family member
/// to access a specific medical record carrying a protected care category.
/// </summary>
/// <remarks>
/// Only the minor patient may sign, and only over services the minor could consent to
/// under Family Code, Division 11, Part 4, as required by
/// <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=CIV&sectionNum=56.11">Cal. Civ. Code § 56.11(b)(3)(A)</see>.
/// The legal representative cannot sign authorizations over those services, per
/// <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=CIV&sectionNum=56.11">Cal. Civ. Code § 56.11(b)(3)(B)</see>.
/// This grant is consent evidence and is never deleted. Revocation takes effect at the
/// instant it is recorded, following
/// <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=CIV&sectionNum=56.15">Cal. Civ. Code § 56.15</see>.
/// It is not retroactive and no notice is sent to the family member.
/// </remarks>
public class MedicalRecordConsentGrant : BaseEntity
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

    public Guid MedicalRecordId { get; private set; }

    public Guid SignedByUserId { get; private set; }

    public DateTime SignedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    /// <remarks>
    /// Null while the grant is in force. Populated when the patient revokes it.
    /// </remarks>
    public DateTime? RevokedAt { get; private set; }

    // EF Core constructor
    private MedicalRecordConsentGrant() { }

    private MedicalRecordConsentGrant(
        Guid patientId,
        Guid recipientMembershipId,
        Guid medicalRecordId,
        Guid signedByUserId,
        DateTime signedAt
    )
        : this()
    {
        PatientId = patientId;
        RecipientMembershipId = recipientMembershipId;
        MedicalRecordId = medicalRecordId;
        SignedByUserId = signedByUserId;
        SignedAt = signedAt;
        ExpiresAt = signedAt.AddYears(ValidityYears);
    }

    internal static MedicalRecordConsentGrant Create(
        Guid patientId,
        Guid recipientMembershipId,
        Guid medicalRecordId,
        Guid signedByUserId,
        DateTime signedAt
    )
    {
        Guard.NotEmpty(patientId);
        Guard.NotEmpty(recipientMembershipId);
        Guard.NotEmpty(medicalRecordId);
        Guard.NotEmpty(signedByUserId);
        Guard.NotDefault(signedAt);

        return new MedicalRecordConsentGrant(
            patientId,
            recipientMembershipId,
            medicalRecordId,
            signedByUserId,
            signedAt
        );
    }

    /// <summary>
    /// Reports whether the grant itself is in force at the given instant.
    /// A grant is in force when it has not been revoked and the instant precedes its expiry.
    /// </summary>
    public bool IsEffectiveAt(DateTime referenceTime)
    {
        Guard.NotDefault(referenceTime);

        return RevokedAt is null && referenceTime < ExpiresAt;
    }

    /// <summary>
    /// Revokes the grant at the given instant. Revocation remains allowed after the patient
    /// reaches adulthood. Takes effect immediately and is not retroactive.
    /// </summary>
    /// <param name="requesterIsAuthorized">
    /// Whether the requester is the patient acting through their active Self membership.
    /// </param>
    public void Revoke(bool requesterIsAuthorized, DateTime referenceTime)
    {
        Guard.NotDefault(referenceTime);

        if (!requesterIsAuthorized)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.UnauthorizedRevocation
            );

        if (RevokedAt is not null)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.AlreadyRevoked
            );

        if (referenceTime >= ExpiresAt)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.AlreadyExpired
            );

        if (referenceTime <= SignedAt)
            throw new DomainValidationException(
                DomainErrors.Validation.EndTimeMustBeAfterStartTime
            );

        RevokedAt = referenceTime;
    }
}

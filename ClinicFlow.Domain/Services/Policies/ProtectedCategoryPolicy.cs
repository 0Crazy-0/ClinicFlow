using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Policies;

/// <summary>
/// Provides the statutory minimum consent ages for protected care categories under
/// California Family Code, Division 11, Part 4, Chapter 3, Consent by Minor.
/// </summary>
public static class ProtectedCategoryPolicy
{
    /// <summary>
    /// Maximum age at which the protected minor consent regime applies. Once the patient reaches
    /// the age of majority, consent reverts entirely to the patient and the FamilyMembership
    /// access level governs family access, so no category is protected anymore.
    /// </summary>
    private const int PrivateConsentAgeOfMajority = 18;

    /// <summary>
    /// Determines the statutory minimum age in years at which a minor can consent to the given
    /// protected care category.
    /// </summary>
    /// <param name="category">The protected care category to evaluate.</param>
    /// <returns>
    /// The statutory minimum age in years. Zero means the statute sets no age threshold, so the
    /// minor holds an absolute right to confidential care from birth: no parent or guardian can
    /// access the clinical record for that category, regardless of the patient's age.
    /// </returns>
    /// <remarks>
    /// Derived from California Family Code, Division 11, Part 4, Chapter 3, Consent by Minor.
    /// Note that some statutes additionally require the attending professional person to deem the
    /// minor mature enough to participate intelligently in the services. That condition is out of
    /// scope for this policy, which only models the age threshold.
    /// <list type="table">
    ///   <listheader>
    ///     <term>Categories</term>
    ///     <description>Minimum age and statutory source</description>
    ///   </listheader>
    ///   <item>
    ///     <term><see cref="ProtectedCategory.MentalHealthCounseling"/>, <see cref="ProtectedCategory.ResidentialShelter"/></term>
    ///     <description>
    ///       12. Outpatient mental health treatment or counseling and residential shelter services
    ///       for minors 12 years of age or older who are mature enough to participate intelligently:
    ///       <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&sectionNum=6924">Cal. Fam. Code § 6924(b)</see>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ProtectedCategory.PregnancyPrevention"/></term>
    ///     <description>
    ///       0. Medical care related to the prevention of pregnancy, except sterilization. The
    ///       minor holds an absolute right to confidential care from birth: no minimum age
    ///       applies and no parent or guardian can access the clinical record:
    ///       <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&sectionNum=6925">Cal. Fam. Code § 6925</see>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ProtectedCategory.CommunicableDisease"/>, <see cref="ProtectedCategory.STIPreventionOrTreatment"/></term>
    ///     <description>
    ///       12. Medical care and counseling related to the diagnosis or treatment of infectious,
    ///       communicable, or reportable diseases, including sexually transmitted infections,
    ///       for minors 12 years of age or older:
    ///       <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&sectionNum=6926">Cal. Fam. Code § 6926</see>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ProtectedCategory.RapeCare"/></term>
    ///     <description>
    ///       12. Medical care and counseling related to the diagnosis and treatment of a rape
    ///       victim for minors 12 years of age or older:
    ///       <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&sectionNum=6927">Cal. Fam. Code § 6927</see>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ProtectedCategory.SexualAssaultCare"/></term>
    ///     <description>
    ///       0. Medical care related to the diagnosis or treatment of a sexual assault and the
    ///       collection of medical evidence. The minor holds an absolute right to confidential
    ///       care from birth: no minimum age applies and no parent or guardian can access the
    ///       clinical record:
    ///       <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&sectionNum=6928">Cal. Fam. Code § 6928</see>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ProtectedCategory.SubstanceAbuseTreatment"/></term>
    ///     <description>
    ///       12. Medical care and counseling for alcohol or drug abuse problems for minors
    ///       12 years of age or older, except opioid use disorder treatment governed by
    ///       § 6929(e)(2) and § 6929.1:
    ///       <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&sectionNum=6929">Cal. Fam. Code § 6929</see>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ProtectedCategory.BuprenorphineOpioidTreatment"/>, <see cref="ProtectedCategory.NarcoticTreatmentProgram"/></term>
    ///     <description>
    ///       16. Buprenorphine opioid use disorder treatment at a physician's office, clinic, or
    ///       health facility for minors 16 years of age or older under
    ///       <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&sectionNum=6929.1">Cal. Fam. Code § 6929.1</see>,
    ///       and medications for opioid use disorder in a licensed narcotic treatment program for
    ///       minors 16 years of age or older as expressly permitted by federal law under
    ///       <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&sectionNum=6929">§ 6929(e)(2)</see>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="ProtectedCategory.IntimatePartnerViolenceCare"/></term>
    ///     <description>
    ///       12. Medical care related to intimate partner violence injuries and the collection of
    ///       medical evidence for minors 12 years of age or older. Does not apply when the minor
    ///       is an alleged victim of rape or sexual assault, in which case §§ 6927 and 6928 apply
    ///       instead:
    ///       <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=FAM&sectionNum=6930">Cal. Fam. Code § 6930</see>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static int MinimumConsentAge(ProtectedCategory category) =>
        category switch
        {
            ProtectedCategory.MentalHealthCounseling => 12,
            ProtectedCategory.ResidentialShelter => 12,
            ProtectedCategory.PregnancyPrevention => 0,
            ProtectedCategory.CommunicableDisease => 12,
            ProtectedCategory.STIPreventionOrTreatment => 12,
            ProtectedCategory.SexualAssaultCare => 0,
            ProtectedCategory.RapeCare => 12,
            ProtectedCategory.SubstanceAbuseTreatment => 12,
            ProtectedCategory.BuprenorphineOpioidTreatment => 16,
            ProtectedCategory.NarcoticTreatmentProgram => 16,
            ProtectedCategory.IntimatePartnerViolenceCare => 12,
            _ => throw new ArgumentOutOfRangeException(nameof(category)),
        };

    /// <summary>
    /// Determines whether a protected care category shields the patient's clinical record from
    /// family member access. Protection applies only while the patient is a minor: the patient
    /// must have reached the statutory minimum consent age for the category and must still be
    /// younger than the age of majority. A category with a minimum age of zero is protected for
    /// patients of any age under the age of majority. A null category is never protected because
    /// no protected consent regime applies to ordinary care.
    /// </summary>
    /// <seealso cref="MinimumConsentAge"/>
    public static bool IsProtectedForPatient(ProtectedCategory? category, int patientAge) =>
        category is not null
        && patientAge >= MinimumConsentAge(category.Value)
        && patientAge < PrivateConsentAgeOfMajority;

    /// <summary>
    /// Collects every protected care category whose statutory minimum consent age is reached by
    /// the patient's age, meaning the category is shielded from family member access. Returns an
    /// empty list once the patient has reached the age of majority, because consent reverts
    /// entirely to the patient and no category is protected anymore.
    /// </summary>
    /// <seealso cref="IsProtectedForPatient"/>
    public static IReadOnlyList<ProtectedCategory> GetProtectedCategoriesFor(int patientAge) =>
        [
            .. Enum.GetValues<ProtectedCategory>()
                .Where(category => IsProtectedForPatient(category, patientAge)),
        ];
}

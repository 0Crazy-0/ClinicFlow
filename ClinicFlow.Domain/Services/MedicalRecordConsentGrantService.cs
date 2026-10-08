using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Consent;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Services.Policies;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Authorizes a family member to access a specific protected medical record on behalf
/// of a minor patient, following the patient's own written authorization.
/// </summary>
/// <remarks>
/// Only the minor patient may sign, and only over services the minor could consent to
/// under Family Code, Division 11, Part 4, as required by
/// <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=CIV&sectionNum=56.11">Cal. Civ. Code § 56.11(b)(3)(A)</see>.
/// The legal representative cannot sign authorizations over those services, per
/// <see href="https://leginfo.legislature.ca.gov/faces/codes_displaySection.xhtml?lawCode=CIV&sectionNum=56.11">Cal. Civ. Code § 56.11(b)(3)(B)</see>.
/// </remarks>
public static class MedicalRecordConsentGrantService
{
    public static MedicalRecordConsentGrant Grant(
        MedicalRecordConsentGrantContext context,
        GrantMedicalRecordConsentArgs args
    )
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(context.Patient);
        ArgumentNullException.ThrowIfNull(context.RequesterMembership);
        ArgumentNullException.ThrowIfNull(context.MedicalRecord);
        ArgumentNullException.ThrowIfNull(context.RecipientMembership);

        context.RequesterMembership.EnsureConsentGrantSigner(
            args.RequesterUserId,
            context.Patient.Id
        );

        var patientAge = context.Patient.GetAge(DateOnly.FromDateTime(args.ReferenceTime));

        if (patientAge >= DomainRules.AdultAge)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.PatientMustBeMinor
            );

        if (context.MedicalRecord.PatientId != context.Patient.Id)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.RecordPatientMismatch
            );

        if (context.MedicalRecord.ProtectedCareCategory is null)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.RecordMustHaveProtectedCategory
            );

        if (
            !ProtectedCategoryPolicy
                .GetProtectedCategoriesFor(patientAge)
                .Contains(context.MedicalRecord.ProtectedCareCategory.Value)
        )
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.CategoryNotGrantable
            );

        context.RecipientMembership.EnsureConsentGrantRecipient(context.Patient.Id);

        if (context.HasEffectiveGrant)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.AlreadyExists
            );

        return MedicalRecordConsentGrant.Create(
            context.Patient.Id,
            context.RecipientMembership.Id,
            context.MedicalRecord.Id,
            context.RequesterMembership.UserId,
            args.ReferenceTime
        );
    }
}

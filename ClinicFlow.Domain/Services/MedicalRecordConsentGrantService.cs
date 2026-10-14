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
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.RequesterMembership);
        ArgumentNullException.ThrowIfNull(context.TargetMedicalRecord);
        ArgumentNullException.ThrowIfNull(context.RecipientMembership);

        context.RequesterMembership.EnsureConsentGrantSigner(
            args.RequesterUserId,
            context.TargetPatient.Id,
            DomainErrors.MedicalRecordConsentGrant.UnauthorizedCreation
        );

        var patientAge = context.TargetPatient.GetAge(DateOnly.FromDateTime(args.ReferenceTime));

        if (patientAge >= DomainRules.AdultAge)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.PatientMustBeMinor
            );

        if (context.TargetMedicalRecord.PatientId != context.TargetPatient.Id)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.RecordPatientMismatch
            );

        if (context.TargetMedicalRecord.ProtectedCareCategory is null)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.RecordMustHaveProtectedCategory
            );

        if (
            !ProtectedCategoryPolicy
                .GetProtectedCategoriesFor(patientAge)
                .Contains(context.TargetMedicalRecord.ProtectedCareCategory.Value)
        )
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.CategoryNotGrantable
            );

        context.RecipientMembership.EnsureConsentGrantRecipient(
            context.TargetPatient.Id,
            DomainErrors.MedicalRecordConsentGrant.RecipientMustBeActive,
            DomainErrors.MedicalRecordConsentGrant.RecipientPatientMismatch,
            DomainErrors.MedicalRecordConsentGrant.RecipientCannotBeSelf
        );

        if (context.HasEffectiveGrant)
            throw new DomainValidationException(
                DomainErrors.MedicalRecordConsentGrant.AlreadyExists
            );

        return MedicalRecordConsentGrant.Create(
            context.TargetPatient.Id,
            context.RecipientMembership.Id,
            context.TargetMedicalRecord.Id,
            context.RequesterMembership.UserId,
            args.ReferenceTime
        );
    }
}

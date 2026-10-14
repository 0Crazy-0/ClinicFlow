using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Consent;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Services.Policies;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Authorizes a family member to see a minor patient's protected appointments,
/// either of a whole protected care category or of a specific appointment,
/// following the patient's own written authorization.
/// </summary>
public static class AppointmentConsentGrantService
{
    public static AppointmentConsentGrant GrantCategory(
        AppointmentCategoryConsentGrantContext context,
        GrantAppointmentCategoryConsentArgs args
    )
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.RequesterMembership);
        ArgumentNullException.ThrowIfNull(context.RecipientMembership);

        context.RequesterMembership.EnsureConsentGrantSigner(
            args.RequesterUserId,
            context.TargetPatient.Id,
            DomainErrors.AppointmentConsentGrant.UnauthorizedCreation
        );

        var patientAge = context.TargetPatient.GetAge(DateOnly.FromDateTime(args.ReferenceTime));

        if (patientAge >= DomainRules.AdultAge)
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.PatientMustBeMinor
            );

        if (
            !ProtectedCategoryPolicy
                .GetProtectedCategoriesFor(patientAge)
                .Contains(context.ProtectedCareCategory)
        )
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.CategoryNotGrantable
            );

        context.RecipientMembership.EnsureConsentGrantRecipient(
            context.TargetPatient.Id,
            DomainErrors.AppointmentConsentGrant.RecipientMustBeActive,
            DomainErrors.AppointmentConsentGrant.RecipientPatientMismatch,
            DomainErrors.AppointmentConsentGrant.RecipientCannotBeSelf
        );

        if (context.HasEffectiveGrant)
            throw new DomainValidationException(DomainErrors.AppointmentConsentGrant.AlreadyExists);

        return AppointmentConsentGrant.CreateForCategory(
            context.TargetPatient.Id,
            context.RecipientMembership.Id,
            context.ProtectedCareCategory,
            context.RequesterMembership.UserId,
            args.ReferenceTime
        );
    }

    public static AppointmentConsentGrant GrantAppointment(
        SpecificAppointmentConsentGrantContext context,
        GrantSpecificAppointmentConsentArgs args
    )
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.RequesterMembership);
        ArgumentNullException.ThrowIfNull(context.RecipientMembership);
        ArgumentNullException.ThrowIfNull(context.TargetAppointment);
        ArgumentNullException.ThrowIfNull(context.AppointmentType);

        context.RequesterMembership.EnsureConsentGrantSigner(
            args.RequesterUserId,
            context.TargetPatient.Id,
            DomainErrors.AppointmentConsentGrant.UnauthorizedCreation
        );

        var patientAge = context.TargetPatient.GetAge(DateOnly.FromDateTime(args.ReferenceTime));

        if (patientAge >= DomainRules.AdultAge)
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.PatientMustBeMinor
            );

        if (context.TargetAppointment.PatientId != context.TargetPatient.Id)
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.AppointmentPatientMismatch
            );

        if (context.AppointmentType.Id != context.TargetAppointment.AppointmentTypeId)
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.AppointmentPatientMismatch
            );

        var category = context.AppointmentType.ProtectedCareCategory;

        if (
            category is null
            || !ProtectedCategoryPolicy
                .GetProtectedCategoriesFor(patientAge)
                .Contains(category.Value)
        )
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.AppointmentNotGrantable
            );

        context.RecipientMembership.EnsureConsentGrantRecipient(
            context.TargetPatient.Id,
            DomainErrors.AppointmentConsentGrant.RecipientMustBeActive,
            DomainErrors.AppointmentConsentGrant.RecipientPatientMismatch,
            DomainErrors.AppointmentConsentGrant.RecipientCannotBeSelf
        );

        if (context.HasEffectiveGrant)
            throw new DomainValidationException(DomainErrors.AppointmentConsentGrant.AlreadyExists);

        return AppointmentConsentGrant.CreateForAppointment(
            context.TargetPatient.Id,
            context.RecipientMembership.Id,
            context.TargetAppointment.Id,
            context.RequesterMembership.UserId,
            args.ReferenceTime
        );
    }
}

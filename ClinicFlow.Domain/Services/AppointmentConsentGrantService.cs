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
        ArgumentNullException.ThrowIfNull(context.Patient);
        ArgumentNullException.ThrowIfNull(context.RequesterMembership);
        ArgumentNullException.ThrowIfNull(context.RecipientMembership);

        context.RequesterMembership.EnsureConsentGrantSigner(
            args.RequesterUserId,
            context.Patient.Id
        );

        var patientAge = context.Patient.GetAge(DateOnly.FromDateTime(args.ReferenceTime));

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

        context.RecipientMembership.EnsureConsentGrantRecipient(context.Patient.Id);

        if (context.HasEffectiveGrant)
            throw new DomainValidationException(DomainErrors.AppointmentConsentGrant.AlreadyExists);

        return AppointmentConsentGrant.CreateForCategory(
            context.Patient.Id,
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
        ArgumentNullException.ThrowIfNull(context.Patient);
        ArgumentNullException.ThrowIfNull(context.RequesterMembership);
        ArgumentNullException.ThrowIfNull(context.RecipientMembership);
        ArgumentNullException.ThrowIfNull(context.Appointment);
        ArgumentNullException.ThrowIfNull(context.AppointmentType);

        context.RequesterMembership.EnsureConsentGrantSigner(
            args.RequesterUserId,
            context.Patient.Id
        );

        var patientAge = context.Patient.GetAge(DateOnly.FromDateTime(args.ReferenceTime));

        if (patientAge >= DomainRules.AdultAge)
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.PatientMustBeMinor
            );

        if (context.Appointment.PatientId != context.Patient.Id)
            throw new DomainValidationException(
                DomainErrors.AppointmentConsentGrant.AppointmentPatientMismatch
            );

        if (context.AppointmentType.Id != context.Appointment.AppointmentTypeId)
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

        context.RecipientMembership.EnsureConsentGrantRecipient(context.Patient.Id);

        if (context.HasEffectiveGrant)
            throw new DomainValidationException(DomainErrors.AppointmentConsentGrant.AlreadyExists);

        return AppointmentConsentGrant.CreateForAppointment(
            context.Patient.Id,
            context.RecipientMembership.Id,
            context.Appointment.Id,
            context.RequesterMembership.UserId,
            args.ReferenceTime
        );
    }
}

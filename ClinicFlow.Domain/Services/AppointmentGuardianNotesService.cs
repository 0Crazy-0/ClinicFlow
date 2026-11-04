using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.GuardianNotes;
using ClinicFlow.Domain.Services.Contexts.GuardianNotes;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Replaces the guardian note of an appointment. First notes require guardian access,
/// edits require the original author, and the patient must be a minor at the scheduled date.
/// </summary>
public static class AppointmentGuardianNotesService
{
    public static void UpdateByGuardian(
        Appointment appointment,
        UpdateGuardianNotesArgs args,
        UpdateGuardianNotesContext context
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.InitiatorMembership);

        if (
            context.TargetPatient.Id != appointment.PatientId
            || context.InitiatorMembership.PatientId != appointment.PatientId
        )
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        if (appointment.GuardianNotesAuthorUserId is null)
        {
            context.InitiatorMembership.EnsureGuardianAccess();
        }
        else
        {
            context.InitiatorMembership.EnsureGuardianNotesEditAccess(
                appointment.GuardianNotesAuthorUserId.Value
            );
        }

        if (
            context.TargetPatient.GetAge(appointment.ScheduledDate)
            >= FamilyMembership.MinimumAdultAge
        )
            throw new DomainValidationException(DomainErrors.Appointment.GuardianRequiresMinor);

        appointment.UpdateGuardianNotes(args.Notes, context.InitiatorMembership.UserId);
    }

    /// <remarks>
    /// an already empty note is an authorized no-op because there is
    /// nothing owned to clear. The appointment status gate stays inside the entity.
    /// </remarks>
    public static void ClearByGuardian(Appointment appointment, UpdateGuardianNotesContext context)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.InitiatorMembership);

        if (
            context.TargetPatient.Id != appointment.PatientId
            || context.InitiatorMembership.PatientId != appointment.PatientId
        )
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        if (appointment.GuardianNotes.Length > 0)
        {
            // A non-empty note always carries its author; the bang documents the invariant.
            context.InitiatorMembership.EnsureGuardianNotesEditAccess(
                appointment.GuardianNotesAuthorUserId!.Value
            );

            if (
                context.TargetPatient.GetAge(appointment.ScheduledDate)
                >= FamilyMembership.MinimumAdultAge
            )
                throw new DomainValidationException(DomainErrors.Appointment.GuardianRequiresMinor);
        }

        appointment.ClearGuardianNotes();
    }
}

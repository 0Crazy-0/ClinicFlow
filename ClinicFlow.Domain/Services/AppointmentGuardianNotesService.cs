using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.GuardianNotes;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Replaces the guardian note of an appointment. First notes require guardian access,
/// edits require the original author, and the patient must be a minor at the scheduled date.
/// </summary>
public static class AppointmentGuardianNotesService
{
    public static void UpdateByGuardian(Appointment appointment, UpdateGuardianNotesArgs args)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.TargetPatient);
        ArgumentNullException.ThrowIfNull(args.InitiatorMembership);

        if (
            args.TargetPatient.Id != appointment.PatientId
            || args.InitiatorMembership.PatientId != appointment.PatientId
        )
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        if (appointment.GuardianNotesAuthorUserId is null)
        {
            args.InitiatorMembership.EnsureGuardianAccess();
        }
        else
        {
            args.InitiatorMembership.EnsureGuardianNotesEditAccess(
                appointment.GuardianNotesAuthorUserId.Value
            );
        }

        if (
            args.TargetPatient.GetAge(appointment.ScheduledDate) >= FamilyMembership.MinimumAdultAge
        )
            throw new DomainValidationException(DomainErrors.Appointment.GuardianRequiresMinor);

        appointment.SetGuardianNotes(args.Notes, args.InitiatorUserId);
    }
}

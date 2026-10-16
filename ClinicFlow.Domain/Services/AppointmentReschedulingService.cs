using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts.Rescheduling;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Orchestrates appointment rescheduling for different actors (Patient, Guardian, Doctor, Staff).
/// </summary>
/// <remarks>
/// All rescheduling requests must be accompanied by a valid scheduling clearance to ensure compliance with regional scheduling regulations.
/// </remarks>
public static class AppointmentReschedulingService
{
    public static void RescheduleByPatient(
        Appointment appointment,
        PatientReschedulingArgs args,
        PatientReschedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.NewTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.AppointmentType);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.InitiatorMembership);
        ArgumentNullException.ThrowIfNull(context.ClinicTimeZone);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Reschedule.MissingClearance);

        DaylightTransitionPolicy.EnsureSchedulable(
            args.NewDate,
            args.NewTimeSlot,
            context.ClinicTimeZone
        );

        if (context.TargetPatient.Id != appointment.PatientId)
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        context.InitiatorMembership.EnsureAppointmentAccess(context.AppointmentType.Category);

        if (
            context.TargetPatient.GetAge(args.NewDate) < FamilyMembership.MinimumAdultAge
            && appointment.ScheduledByUserId != context.InitiatorMembership.UserId
            && context.CreatorLegalAuthority
                is LegalAuthorityType.Parent
                    or LegalAuthorityType.Guardian
        )
            throw new PatientAccessUnauthorizedException(DomainErrors.Patient.UnauthorizedAccess);

        if (!context.IsInitiatorPhoneVerified)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.PhoneNotVerified
            );

        new PenaltyHistory(context.Penalties).EnsureNotBlocked(args.NewDate);

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            appointment.DoctorId,
            args.NewDate.DayOfWeek,
            args.NewTimeSlot
        );

        appointment.Reschedule(args.NewDate, args.NewTimeSlot);

        if (args.NewPatientNotes is not null)
            appointment.UpdatePatientNotes(args.NewPatientNotes);
    }

    public static void RescheduleByGuardian(
        Appointment appointment,
        GuardianReschedulingArgs args,
        PatientReschedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.NewTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.TargetPatient);
        ArgumentNullException.ThrowIfNull(context.AppointmentType);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.InitiatorMembership);
        ArgumentNullException.ThrowIfNull(context.ClinicTimeZone);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Reschedule.MissingClearance);

        DaylightTransitionPolicy.EnsureSchedulable(
            args.NewDate,
            args.NewTimeSlot,
            context.ClinicTimeZone
        );

        if (context.TargetPatient.Id != appointment.PatientId)
            throw new DomainValidationException(DomainErrors.Appointment.DataMismatch);

        context.InitiatorMembership.EnsureGuardianAccess();

        var isMinorAtNewDate =
            context.TargetPatient.GetAge(args.NewDate) < FamilyMembership.MinimumAdultAge;

        if (!isMinorAtNewDate)
            throw new DomainValidationException(DomainErrors.Appointment.GuardianRequiresMinor);

        if (args.NewGuardianNotes is not null && appointment.GuardianNotesAuthorUserId is not null)
            context.InitiatorMembership.EnsureGuardianNotesEditAccess(
                appointment.GuardianNotesAuthorUserId.Value
            );

        if (!context.IsInitiatorPhoneVerified)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.PhoneNotVerified
            );

        new PenaltyHistory(context.Penalties).EnsureNotBlocked(args.NewDate);

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            appointment.DoctorId,
            args.NewDate.DayOfWeek,
            args.NewTimeSlot
        );

        appointment.Reschedule(args.NewDate, args.NewTimeSlot);

        if (args.NewGuardianNotes is not null)
            appointment.UpdateGuardianNotes(
                args.NewGuardianNotes,
                context.InitiatorMembership.UserId
            );
    }

    public static void RescheduleByDoctor(
        Appointment appointment,
        DoctorReschedulingArgs args,
        DoctorReschedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.NewTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.InitiatorDoctor);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.ClinicTimeZone);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Reschedule.MissingClearance);

        DaylightTransitionPolicy.EnsureSchedulable(
            args.NewDate,
            args.NewTimeSlot,
            context.ClinicTimeZone
        );

        if (context.InitiatorDoctor.Id != appointment.DoctorId)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedScheduling
            );

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            appointment.DoctorId,
            args.NewDate.DayOfWeek,
            args.NewTimeSlot
        );

        appointment.Reschedule(args.NewDate, args.NewTimeSlot);
    }

    public static void RescheduleByStaff(
        Appointment appointment,
        StaffReschedulingArgs args,
        StaffReschedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.NewTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.ClinicTimeZone);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Reschedule.MissingClearance);

        DaylightTransitionPolicy.EnsureSchedulable(
            args.NewDate,
            args.NewTimeSlot,
            context.ClinicTimeZone
        );

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            appointment.DoctorId,
            args.NewDate.DayOfWeek,
            args.NewTimeSlot
        );

        appointment.Reschedule(args.NewDate, args.NewTimeSlot);
    }
}

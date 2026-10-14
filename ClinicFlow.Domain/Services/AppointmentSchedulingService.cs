using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Orchestrates appointment scheduling for different actors (Patient, Guardian, Doctor, Staff).
/// </summary>
/// <remarks>
/// All scheduling requests must be accompanied by a valid scheduling clearance to ensure compliance with regional scheduling regulations.
/// </remarks>
public static class AppointmentSchedulingService
{
    public static Appointment ScheduleByPatient(
        AppointmentTypeDefinition appointmentType,
        PatientSchedulingArgs args,
        PatientSchedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointmentType);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.TargetPatient);
        ArgumentNullException.ThrowIfNull(args.RequestedTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.InitiatorMembership);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        context.InitiatorMembership.EnsureAppointmentAccess(context.RequestedCategory);

        if (!args.IsInitiatorPhoneVerified)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.PhoneNotVerified
            );

        args.TargetPatient.EnsureCompleteProfile();

        new PenaltyHistory(context.Penalties).EnsureNotBlocked(args.ScheduledDate);

        appointmentType.ValidatePatientEligibility(args.TargetPatient.GetAge(args.ScheduledDate));

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            args.RequestedTimeSlot
        );

        var appointment = Appointment.Schedule(
            args.TargetPatient.Id,
            args.DoctorId,
            appointmentType.Id,
            args.ScheduledDate,
            args.RequestedTimeSlot,
            args.InitiatorUserId
        );

        if (args.PatientNotes is not null)
            appointment.UpdatePatientNotes(args.PatientNotes);

        return appointment;
    }

    public static Appointment ScheduleByGuardian(
        AppointmentTypeDefinition appointmentType,
        GuardianSchedulingArgs args,
        PatientSchedulingContext context,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointmentType);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.TargetPatient);
        ArgumentNullException.ThrowIfNull(args.RequestedTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.DoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.InitiatorMembership);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        context.InitiatorMembership.EnsureGuardianAccess();

        if (!args.IsInitiatorPhoneVerified)
            throw new AppointmentSchedulingUnauthorizedException(
                DomainErrors.Appointment.PhoneNotVerified
            );

        args.TargetPatient.EnsureCompleteProfile();

        new PenaltyHistory(context.Penalties).EnsureNotBlocked(args.ScheduledDate);

        appointmentType.ValidatePatientEligibility(
            args.TargetPatient.GetAge(args.ScheduledDate),
            true
        );

        context.DoctorSchedule.EnsureDoctorIsAvailable(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            args.RequestedTimeSlot
        );

        var isMinorAtScheduledDate =
            args.TargetPatient.GetAge(args.ScheduledDate) < FamilyMembership.MinimumAdultAge;

        if (!isMinorAtScheduledDate)
            throw new DomainValidationException(DomainErrors.Appointment.GuardianRequiresMinor);

        var appointment = Appointment.Schedule(
            args.TargetPatient.Id,
            args.DoctorId,
            appointmentType.Id,
            args.ScheduledDate,
            args.RequestedTimeSlot,
            args.InitiatorUserId
        );

        if (args.GuardianNotes is not null)
            appointment.UpdateGuardianNotes(args.GuardianNotes, args.InitiatorUserId);

        return appointment;
    }

    public static Appointment ScheduleByDoctor(
        AppointmentTypeDefinition appointmentType,
        DoctorSchedulingArgs args,
        Schedule doctorSchedule,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointmentType);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.InitiatorDoctor);
        ArgumentNullException.ThrowIfNull(args.TargetPatient);
        ArgumentNullException.ThrowIfNull(args.RequestedTimeSlot);
        ArgumentNullException.ThrowIfNull(doctorSchedule);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        appointmentType.ValidatePatientEligibility(
            args.TargetPatient.GetAge(args.ScheduledDate),
            args.HasGuardianConsentVerified
        );

        doctorSchedule.EnsureDoctorIsAvailable(
            args.InitiatorDoctor.Id,
            args.ScheduledDate.DayOfWeek,
            args.RequestedTimeSlot
        );

        return Appointment.Schedule(
            args.TargetPatient.Id,
            args.InitiatorDoctor.Id,
            appointmentType.Id,
            args.ScheduledDate,
            args.RequestedTimeSlot,
            args.InitiatorDoctor.UserId
        );
    }

    public static Appointment ScheduleByStaff(
        AppointmentTypeDefinition appointmentType,
        StaffSchedulingArgs args,
        Schedule doctorSchedule,
        SchedulingClearance clearance
    )
    {
        ArgumentNullException.ThrowIfNull(appointmentType);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.TargetPatient);
        ArgumentNullException.ThrowIfNull(args.RequestedTimeSlot);
        ArgumentNullException.ThrowIfNull(doctorSchedule);

        if (clearance is null)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance);

        args.TargetPatient.EnsureCompleteProfile();

        appointmentType.ValidatePatientEligibility(
            args.TargetPatient.GetAge(args.ScheduledDate),
            args.HasGuardianConsentVerified
        );

        doctorSchedule.EnsureDoctorIsAvailable(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            args.RequestedTimeSlot
        );

        return Appointment.Schedule(
            args.TargetPatient.Id,
            args.DoctorId,
            appointmentType.Id,
            args.ScheduledDate,
            args.RequestedTimeSlot,
            args.InitiatorUserId
        );
    }
}

using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services.Args.Reassignment;
using ClinicFlow.Domain.Services.Contexts;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Reassigns a displaced appointment to a new doctor after a doctor suspension.
/// Enforces availability and conflict rules but intentionally ignores
/// patient penalties and administrative time limits.
/// </summary>
public static class AppointmentReassignmentService
{
    public static void Reassign(
        Appointment appointment,
        AppointmentReassignmentArgs args,
        AppointmentReassignmentContext context
    )
    {
        if (appointment is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (context is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (args is null || args.NewTimeRange is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (
            context.NewDoctorSchedule is null
            || !context.NewDoctorSchedule.CoversTimeRange(args.NewTimeRange)
        )
            throw new DoctorNotAvailableException(
                DomainErrors.Schedule.DoctorNotAvailable,
                args.NewDoctorId,
                args.NewDate.DayOfWeek
            );

        if (context.HasConflict)
            throw new AppointmentConflictException(
                DomainErrors.Appointment.Conflict,
                args.NewDoctorId,
                args.NewDate.Add(args.NewTimeRange.Start)
            );

        appointment.Reassign(args.NewDoctorId, args.NewDate, args.NewTimeRange);
    }
}

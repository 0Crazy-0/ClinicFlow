using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Services.Args.Reassignment;
using ClinicFlow.Domain.Services.Contexts.Reassignment;
using ClinicFlow.Domain.Services.Policies;

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
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.NewTimeSlot);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.NewDoctorSchedule);
        ArgumentNullException.ThrowIfNull(context.ClinicTimeZone);

        DaylightTransitionPolicy.EnsureSchedulable(
            args.NewDate,
            args.NewTimeSlot,
            context.ClinicTimeZone
        );

        context.NewDoctorSchedule.EnsureDoctorIsAvailable(
            args.NewDoctorId,
            args.NewDate.DayOfWeek,
            args.NewTimeSlot
        );

        appointment.Reassign(args.NewDoctorId, args.NewDate, args.NewTimeSlot);
    }
}

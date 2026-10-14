using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Services.Args.Reassignment;
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
        Schedule newDoctorSchedule
    )
    {
        ArgumentNullException.ThrowIfNull(appointment);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.NewTimeSlot);
        ArgumentNullException.ThrowIfNull(args.ClinicTimeZone);
        ArgumentNullException.ThrowIfNull(newDoctorSchedule);

        DaylightTransitionPolicy.EnsureSchedulable(
            args.NewDate,
            args.NewTimeSlot,
            args.ClinicTimeZone
        );

        newDoctorSchedule.EnsureDoctorIsAvailable(
            args.NewDoctorId,
            args.NewDate.DayOfWeek,
            args.NewTimeSlot
        );

        appointment.Reassign(args.NewDoctorId, args.NewDate, args.NewTimeSlot);
    }
}

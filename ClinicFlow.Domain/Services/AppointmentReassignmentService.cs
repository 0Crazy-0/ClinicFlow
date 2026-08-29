using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Services.Args.Reassignment;

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
        ArgumentNullException.ThrowIfNull(args.NewTimeRange);
        ArgumentNullException.ThrowIfNull(newDoctorSchedule);

        newDoctorSchedule.EnsureDoctorIsAvailable(
            args.NewDoctorId,
            args.NewDate.DayOfWeek,
            args.NewTimeRange
        );

        appointment.Reassign(args.NewDoctorId, args.NewDate, args.NewTimeRange);
    }
}

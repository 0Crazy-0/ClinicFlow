using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
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
        if (appointment is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (newDoctorSchedule is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (args is null || args.NewTimeRange is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        newDoctorSchedule.EnsureDoctorIsAvailable(
            args.NewDoctorId,
            args.NewDate.DayOfWeek,
            args.NewTimeRange
        );

        appointment.Reassign(args.NewDoctorId, args.NewDate, args.NewTimeRange);
    }
}

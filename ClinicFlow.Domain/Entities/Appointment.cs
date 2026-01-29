using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid AppointmentTypeId { get; private set; }

    public DateTime ScheduledDate { get; private set; }
    public TimeRange TimeRange { get; private set; }

    public AppointmentStatusEnum Status { get; private set; }
    public string PatientNotes { get; private set; } = string.Empty;
    public string ReceptionistNotes { get; private set; } = string.Empty;

    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public Guid? CancelledByUserId { get; private set; }

    public int RescheduleCount { get; private set; }

    // EF Core constructor
    private Appointment()
    {
        TimeRange = null!;
    }

    private Appointment(Guid patientId, Guid doctorId, Guid appointmentTypeId, DateTime scheduledDate, TimeRange timeRange)
    {
        PatientId = patientId;
        DoctorId = doctorId;
        AppointmentTypeId = appointmentTypeId;
        ScheduledDate = scheduledDate;
        TimeRange = timeRange;
        Status = AppointmentStatusEnum.Scheduled;
        RescheduleCount = 0;
    }

    internal static Appointment Schedule(Guid patientId, Guid doctorId, Guid appointmentTypeId, DateTime scheduledDate, TimeRange timeRange)
    {
        var appointment = new Appointment(patientId, doctorId, appointmentTypeId, scheduledDate, timeRange);

        appointment.AddDomainEvent(new AppointmentScheduledEvent(appointment));

        return appointment;
    }

    public bool CanBeCancelled(int minHoursBeforeAppointment)
    {
        var appointmentDateTime = ScheduledDate.Add(TimeRange.Start);
        var hoursUntilAppointment = (appointmentDateTime - DateTime.UtcNow).TotalHours;
        return hoursUntilAppointment >= minHoursBeforeAppointment;
    }

    public bool CanBeRescheduled() => RescheduleCount < 1 && Status is AppointmentStatusEnum.Scheduled;

    public void Cancel(Guid userId, string? reason, int minHours)
    {
        if (Status is AppointmentStatusEnum.Cancelled || Status is AppointmentStatusEnum.LateCancellation)
            throw new InvalidOperationException($"Cannot cancel appointment. Current status: {Status}");

        if (!CanBeCancelled(minHours))
        {
            Status = AppointmentStatusEnum.LateCancellation;
        }
        else
        {
            Status = AppointmentStatusEnum.Cancelled;
        }

        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = userId;
        CancellationReason = reason;

        AddDomainEvent(new AppointmentCancelledEvent(this, userId, reason));
    }

    public void Confirm()
    {
        if (Status is not AppointmentStatusEnum.Scheduled)
            throw new InvalidOperationException("Only scheduled appointments can be confirmed.");

        Status = AppointmentStatusEnum.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void Reschedule(DateTime newDate, TimeRange newTimeRange, IEnumerable<Appointment> existingDoctorAppointments)
    {
        if (!CanBeRescheduled())
            throw new InvalidOperationException("This appointment cannot be rescheduled.");

        if (HasScheduleConflict(existingDoctorAppointments, newDate, newTimeRange))
            throw new AppointmentConflictException(DoctorId, newDate.Add(newTimeRange.Start));

        var previousDate = ScheduledDate;
        var previousTimeRange = TimeRange;

        ScheduledDate = newDate;
        TimeRange = newTimeRange;
        RescheduleCount++;

        AddDomainEvent(new AppointmentRescheduledEvent(this, previousDate, previousTimeRange));
    }

    internal static bool HasScheduleConflict(IEnumerable<Appointment> appointments, DateTime scheduledDate, TimeRange timeRange) =>
        appointments.Any(a => a.ScheduledDate.Date == scheduledDate.Date && a.IsActive() && a.TimeRange.OverlapsWith(timeRange));

    private bool IsActive() => Status != AppointmentStatusEnum.Cancelled && Status is not AppointmentStatusEnum.LateCancellation;

    public DateTime GetScheduledDateTime() => ScheduledDate.Add(TimeRange.Start);
}

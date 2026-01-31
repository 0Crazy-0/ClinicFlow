using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

public class Appointment : BaseEntity
{
    #region Properties

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

    #endregion

    #region Constructors

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

    #endregion

    #region Factory Methods

    internal static Appointment Schedule(Guid patientId, Guid doctorId, Guid appointmentTypeId, DateTime scheduledDate, TimeRange timeRange)
    {
        var appointment = new Appointment(patientId, doctorId, appointmentTypeId, scheduledDate, timeRange);

        appointment.AddDomainEvent(new AppointmentScheduledEvent(appointment));

        return appointment;
    }

    #endregion

    #region Public Domain Methods

    internal void Cancel(Guid cancelledByUserId, string? reason, int minHours)
    {
        if (Status is AppointmentStatusEnum.Cancelled or AppointmentStatusEnum.LateCancellation)
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
        CancelledByUserId = cancelledByUserId;
        CancellationReason = reason;

        AddDomainEvent(new AppointmentCancelledEvent(this, cancelledByUserId, reason));
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
    #endregion

    #region Business Rules (Private)

    private bool CanBeCancelled(int minHoursBeforeAppointment)
    {
        var appointmentDateTime = ScheduledDate.Add(TimeRange.Start);
        var hoursUntilAppointment = (appointmentDateTime - DateTime.UtcNow).TotalHours;

        return hoursUntilAppointment >= minHoursBeforeAppointment;
    }

    private bool CanBeRescheduled() => RescheduleCount < 1 && Status is AppointmentStatusEnum.Scheduled;

    private bool IsActive() => Status is not AppointmentStatusEnum.Cancelled && Status is not AppointmentStatusEnum.LateCancellation;

    #endregion

    #region Validations (Private)
    private static bool HasScheduleConflict(IEnumerable<Appointment> appointments, DateTime scheduledDate, TimeRange timeRange) =>
        appointments.Any(a => a.ScheduledDate.Date == scheduledDate.Date && (a.IsActive() || a.Status is AppointmentStatusEnum.LateCancellation)
        && a.TimeRange.OverlapsWith(timeRange));

    #endregion
}

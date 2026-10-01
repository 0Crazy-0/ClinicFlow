using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a medical appointment between a patient and a doctor.
/// Enforces lifecycle transitions across <see cref="AppointmentStatus.Scheduled"/>,
/// <see cref="AppointmentStatus.CheckedIn"/>, <see cref="AppointmentStatus.InProgress"/>,
/// <see cref="AppointmentStatus.Completed"/>, <see cref="AppointmentStatus.NoShow"/>,
/// <see cref="AppointmentStatus.Cancelled"/>, <see cref="AppointmentStatus.LateCancellation"/>
/// and <see cref="AppointmentStatus.RequiresReassignment"/>.
/// </summary>
public class Appointment : BaseEntity
{
    public const string SystemTimeoutCancellationReason =
        "System timeout: Displaced appointment was not reassigned.";

    public Guid PatientId { get; init; }

    public Guid DoctorId { get; private set; }

    public Guid AppointmentTypeId { get; init; }

    public DateOnly ScheduledDate { get; private set; }

    public TimeRange TimeRange { get; private set; }

    public AppointmentStatus Status { get; private set; }

    // Stryker disable once String
    public string PatientNotes { get; private set; } = string.Empty;

    // Stryker disable once String
    public string ReceptionistNotes { get; private set; } = string.Empty;

    public DateOnly? CheckedInAt { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public DateOnly? CancelledAt { get; private set; }

    public string? CancellationReason { get; private set; }

    public Guid? CancelledByUserId { get; private set; }

    public int RescheduleCount { get; private set; }

    public Guid ScheduledByUserId { get; private set; }

    // EF Core constructor
    private Appointment() => TimeRange = null!;

    private Appointment(
        Guid patientId,
        Guid doctorId,
        Guid appointmentTypeId,
        DateOnly scheduledDate,
        TimeRange timeRange,
        string? patientNotes,
        Guid scheduledByUserId
    )
    {
        PatientId = patientId;
        DoctorId = doctorId;
        AppointmentTypeId = appointmentTypeId;
        ScheduledDate = scheduledDate;
        TimeRange = timeRange;
        Status = AppointmentStatus.Scheduled;
        RescheduleCount = 0;
        PatientNotes = patientNotes ?? string.Empty;
        ScheduledByUserId = scheduledByUserId;
    }

    internal static Appointment Schedule(
        Guid patientId,
        Guid doctorId,
        Guid appointmentTypeId,
        DateOnly scheduledDate,
        TimeRange timeRange,
        string? patientNotes,
        Guid scheduledByUserId
    )
    {
        Guard.NotEmpty(patientId);
        Guard.NotEmpty(doctorId);
        Guard.NotEmpty(appointmentTypeId);
        Guard.NotNull(timeRange);
        Guard.NotEmpty(scheduledByUserId);

        var appointment = new Appointment(
            patientId,
            doctorId,
            appointmentTypeId,
            scheduledDate,
            timeRange,
            patientNotes,
            scheduledByUserId
        );

        appointment.AddDomainEvent(new AppointmentScheduledEvent(appointment));

        return appointment;
    }

    internal void Reschedule(DateOnly newDate, TimeRange newTimeRange)
    {
        if (RescheduleCount >= 1 || Status is not AppointmentStatus.Scheduled)
            throw new AppointmentReschedulingNotAllowedException(
                DomainErrors.Appointment.CannotReschedule
            );

        var previousDate = ScheduledDate;
        var previousTimeRange = TimeRange;

        ScheduledDate = newDate;
        TimeRange = newTimeRange;
        RescheduleCount++;

        AddDomainEvent(new AppointmentRescheduledEvent(this, previousDate, previousTimeRange));
    }

    internal void Cancel(Guid cancelledByUserId, string? reason, DateOnly cancelledAt)
    {
        EnsureCancellable();

        Status = AppointmentStatus.Cancelled;
        ApplyCancellation(cancelledByUserId, reason, cancelledAt);
        AddDomainEvent(new AppointmentCancelledEvent(this, cancelledByUserId, reason));
    }

    internal void CancelLate(Guid cancelledByUserId, string? reason, DateOnly cancelledAt)
    {
        EnsureCancellable();

        Status = AppointmentStatus.LateCancellation;
        ApplyCancellation(cancelledByUserId, reason, cancelledAt);
        AddDomainEvent(new AppointmentLateCancelledEvent(this, cancelledByUserId, reason));
    }

    /// <remarks>
    /// The check-in must occur on the appointment's scheduled date, before the appointment's end time.
    /// Checking in before the appointment's start time is allowed since patients may arrive early.
    /// </remarks>
    public void CheckIn(DateTime checkedInAt, string? receptionistNotes = null)
    {
        var checkedInDate = DateOnly.FromDateTime(checkedInAt);

        if (checkedInDate != ScheduledDate)
            throw new DomainValidationException(DomainErrors.Appointment.InvalidCheckInDate);

        if (checkedInAt >= ScheduledDate.ToDateTime(TimeRange.End))
            throw new DomainValidationException(DomainErrors.Appointment.CheckInAfterEndTime);

        if (Status is not AppointmentStatus.Scheduled)
            throw new DomainValidationException(DomainErrors.Appointment.CannotCheckIn);

        Status = AppointmentStatus.CheckedIn;
        CheckedInAt = checkedInDate;
        ReceptionistNotes = receptionistNotes ?? string.Empty;

        AddDomainEvent(new AppointmentCheckedInEvent(this, checkedInDate));
    }

    /// <remarks>
    /// The start must occur on the appointment's scheduled date. Starting after the appointment's end
    /// time is allowed, since doctor delays must be recorded as actual events rather than rejected.
    /// </remarks>
    public void Start(Guid initiatorDoctorId, DateTime startedAt)
    {
        if (DateOnly.FromDateTime(startedAt) != ScheduledDate)
            throw new DomainValidationException(DomainErrors.Appointment.InvalidStartDate);

        if (startedAt < ScheduledDate.ToDateTime(TimeRange.Start))
            throw new DomainValidationException(DomainErrors.Appointment.InvalidStartDate);

        if (initiatorDoctorId != DoctorId)
            throw new DomainValidationException(DomainErrors.Appointment.UnauthorizedDoctor);

        if (Status is not AppointmentStatus.CheckedIn)
            throw new DomainValidationException(DomainErrors.Appointment.CannotStart);

        Status = AppointmentStatus.InProgress;
        StartedAt = startedAt;

        AddDomainEvent(new AppointmentStartedEvent(this, startedAt));
    }

    internal void Complete(DateTime completedAt)
    {
        if (completedAt < ScheduledDate.ToDateTime(TimeRange.Start))
            throw new DomainValidationException(DomainErrors.Appointment.InvalidCompletionDate);

        if (StartedAt.HasValue && completedAt < StartedAt.Value)
            throw new DomainValidationException(
                DomainErrors.Appointment.CompletionBeforeActualStart
            );

        if (Status is not AppointmentStatus.InProgress)
            throw new DomainValidationException(DomainErrors.Appointment.CannotComplete);

        Status = AppointmentStatus.Completed;
        CompletedAt = completedAt;

        AddDomainEvent(new AppointmentCompletedEvent(this, completedAt));
    }

    /// <remarks>
    /// This method is invoked during administrative or clinical disruptions.
    /// It places the appointment in a holding queue, ensuring the patient is not penalized
    /// while the staff searches for a new available slot.
    /// </remarks>
    internal void MarkAsRequiresReassignment()
    {
        if (Status is not AppointmentStatus.Scheduled)
            throw new DomainValidationException(DomainErrors.Appointment.CannotReassign);

        Status = AppointmentStatus.RequiresReassignment;
    }

    internal void Reassign(Guid newDoctorId, DateOnly newDate, TimeRange newTimeRange)
    {
        Guard.NotNull(newTimeRange);
        Guard.NotEmpty(newDoctorId);

        if (Status is not AppointmentStatus.RequiresReassignment)
            throw new DomainValidationException(DomainErrors.Appointment.CannotReassign);

        var previousDoctorId = DoctorId;

        DoctorId = newDoctorId;
        ScheduledDate = newDate;
        TimeRange = newTimeRange;
        Status = AppointmentStatus.Scheduled;

        AddDomainEvent(new AppointmentReassignedEvent(this, previousDoctorId));
    }

    /// <remarks>
    /// Cancels a displaced appointment that was never reassigned before its scheduled time.
    /// No patient penalty is applied since the cancellation is due to clinic inaction.
    /// </remarks>
    public void CancelDueToSystemTimeout(DateOnly cancelledAt)
    {
        if (Status is not AppointmentStatus.RequiresReassignment)
            throw new DomainValidationException(DomainErrors.Appointment.CannotCancel);

        Status = AppointmentStatus.Cancelled;
        ApplyCancellation(null, SystemTimeoutCancellationReason, cancelledAt);
        AddDomainEvent(new AppointmentSystemCancelledEvent(this));
    }

    public void MarkAsNoShowByStaff() => MarkAsNoShow();

    public void MarkAsNoShowByDoctor(Guid initiatorDoctorId)
    {
        if (initiatorDoctorId != DoctorId)
            throw new AppointmentNoShowUnauthorizedException(
                DomainErrors.Appointment.UnauthorizedNoShow
            );

        MarkAsNoShow();
    }

    public void UpdatePatientNotes(string? notes)
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.RequiresReassignment))
            throw new DomainValidationException(DomainErrors.Appointment.CannotUpdateNotes);

        PatientNotes = notes ?? string.Empty;
    }

    public void UpdateReceptionistNotes(string? notes)
    {
        if (Status is not AppointmentStatus.CheckedIn)
            throw new DomainValidationException(DomainErrors.Appointment.CannotUpdateNotes);

        ReceptionistNotes = notes ?? string.Empty;
    }

    private void MarkAsNoShow()
    {
        if (Status is not AppointmentStatus.Scheduled)
            throw new DomainValidationException(DomainErrors.Appointment.CannotMarkNoShow);

        Status = AppointmentStatus.NoShow;

        AddDomainEvent(new AppointmentMarkedAsNoShowEvent(this));
    }

    private void EnsureCancellable()
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn))
            throw new AppointmentCancellationNotAllowedException(
                DomainErrors.Appointment.CannotCancel,
                Status
            );
    }

    private void ApplyCancellation(Guid? cancelledByUserId, string? reason, DateOnly cancelledAt)
    {
        CancelledAt = cancelledAt;
        CancelledByUserId = cancelledByUserId;
        CancellationReason = reason;
    }
}

using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a medical appointment between a patient and a doctor.
/// Enforces the full appointment lifecycle transitions: scheduling, rescheduling, check-in, in-progress, completion, no-show, and cancellation.
/// </summary>
public class Appointment : BaseEntity
{
    public const string SystemTimeoutCancellationReason =
        "System timeout: Displaced appointment was not reassigned.";

    public Guid PatientId { get; init; }

    public Guid DoctorId { get; private set; }

    public Guid AppointmentTypeId { get; init; }

    public DateTime ScheduledDate { get; private set; }

    public TimeRange TimeRange { get; private set; }

    public AppointmentStatus Status { get; private set; }

    public string PatientNotes { get; private set; } = string.Empty;

    public string ReceptionistNotes { get; private set; } = string.Empty;

    public DateTime? CheckedInAt { get; private set; }

    public DateTime? CancelledAt { get; private set; }

    public string? CancellationReason { get; private set; }

    public Guid? CancelledByUserId { get; private set; }

    public int RescheduleCount { get; private set; }

    // EF Core constructor
    private Appointment()
    {
        TimeRange = null!;
    }

    private Appointment(
        Guid patientId,
        Guid doctorId,
        Guid appointmentTypeId,
        DateTime scheduledDate,
        TimeRange timeRange
    )
    {
        PatientId = patientId;
        DoctorId = doctorId;
        AppointmentTypeId = appointmentTypeId;
        ScheduledDate = scheduledDate;
        TimeRange = timeRange;
        Status = AppointmentStatus.Scheduled;
        RescheduleCount = 0;
    }

    internal static Appointment Schedule(
        Guid patientId,
        Guid doctorId,
        Guid appointmentTypeId,
        DateTime scheduledDate,
        TimeRange timeRange
    )
    {
        if (patientId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (doctorId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (appointmentTypeId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (timeRange is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        var appointment = new Appointment(
            patientId,
            doctorId,
            appointmentTypeId,
            scheduledDate,
            timeRange
        );

        appointment.AddDomainEvent(new AppointmentScheduledEvent(appointment));

        return appointment;
    }

    internal void Cancel(Guid cancelledByUserId, string? reason, DateTime cancelledAt)
    {
        EnsureCancellable();

        Status = AppointmentStatus.Cancelled;
        ApplyCancellation(cancelledByUserId, reason, cancelledAt);
        AddDomainEvent(new AppointmentCancelledEvent(this, cancelledByUserId, reason));
    }

    internal void CancelLate(Guid cancelledByUserId, string? reason, DateTime cancelledAt)
    {
        EnsureCancellable();

        Status = AppointmentStatus.LateCancellation;
        ApplyCancellation(cancelledByUserId, reason, cancelledAt);
        AddDomainEvent(new AppointmentLateCancelledEvent(this, cancelledByUserId, reason));
    }

    public void CheckIn(DateTime checkedInAt)
    {
        if (Status is not AppointmentStatus.Scheduled)
            throw new DomainValidationException(DomainErrors.Appointment.CannotCheckIn);

        Status = AppointmentStatus.CheckedIn;
        CheckedInAt = checkedInAt;

        AddDomainEvent(new AppointmentCheckedInEvent(this, checkedInAt));
    }

    public void Start(Guid initiatorDoctorId, DateTime startedAt)
    {
        if (initiatorDoctorId != DoctorId)
            throw new DomainValidationException(DomainErrors.Appointment.UnauthorizedDoctor);

        if (Status is not AppointmentStatus.CheckedIn)
            throw new DomainValidationException(DomainErrors.Appointment.CannotStart);

        Status = AppointmentStatus.InProgress;

        AddDomainEvent(new AppointmentStartedEvent(this, startedAt));
    }

    public void Complete(DateTime completedAt)
    {
        if (Status is not AppointmentStatus.InProgress)
            throw new DomainValidationException(DomainErrors.Appointment.CannotComplete);

        Status = AppointmentStatus.Completed;

        AddDomainEvent(new AppointmentCompletedEvent(this, completedAt));
    }

    internal void Reschedule(DateTime newDate, TimeRange newTimeRange)
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

    internal void Reassign(Guid newDoctorId, DateTime newDate, TimeRange newTimeRange)
    {
        if (newTimeRange is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (newDoctorId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

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
    public void CancelDueToSystemTimeout(DateTime cancelledAt)
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

    private void MarkAsNoShow()
    {
        if (Status is not AppointmentStatus.Scheduled)
            throw new DomainValidationException(DomainErrors.Appointment.CannotMarkNoShow);

        Status = AppointmentStatus.NoShow;

        AddDomainEvent(new AppointmentMarkedAsNoShowEvent(this));
    }

    private void EnsureCancellable()
    {
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.LateCancellation)
            throw new AppointmentCancellationNotAllowedException(
                DomainErrors.Appointment.CannotCancel,
                Status
            );
    }

    private void ApplyCancellation(Guid? cancelledByUserId, string? reason, DateTime cancelledAt)
    {
        CancelledAt = cancelledAt;
        CancelledByUserId = cancelledByUserId;
        CancellationReason = reason;
    }
}

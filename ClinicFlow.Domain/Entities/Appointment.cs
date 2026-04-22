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
    public Guid PatientId { get; init; }

    public Guid DoctorId { get; init; }

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
    }

    internal void CancelLate(Guid cancelledByUserId, string? reason, DateTime cancelledAt)
    {
        EnsureCancellable();

        Status = AppointmentStatus.LateCancellation;
        ApplyCancellation(cancelledByUserId, reason, cancelledAt);
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
        if (!CanBeRescheduled())
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

    private void ApplyCancellation(Guid cancelledByUserId, string? reason, DateTime cancelledAt)
    {
        CancelledAt = cancelledAt;
        CancelledByUserId = cancelledByUserId;
        CancellationReason = reason;

        AddDomainEvent(new AppointmentCancelledEvent(this, cancelledByUserId, reason));
    }

    private bool CanBeRescheduled() => RescheduleCount < 1 && Status is AppointmentStatus.Scheduled;
}

using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a medical appointment between a patient and a doctor.
/// Enforces lifecycle transitions (scheduling, confirmation, rescheduling, and cancellation).
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

    /// <summary>
    /// Creates a new appointment in <see cref="AppointmentStatus.Scheduled"/> status and raises an <see cref="AppointmentScheduledEvent"/>.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when any required identifier is empty or the time range is null.</exception>
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

    /// <summary>
    /// Cancels the appointment. If the cancellation falls within the specialty's minimum notice period,
    /// the status is set to <see cref="AppointmentStatus.LateCancellation"/> instead.
    /// </summary>
    /// <param name="specialty">The medical specialty, used to evaluate the cancellation notice policy.</param>
    /// <exception cref="AppointmentCancellationNotAllowedException">Thrown when the appointment is already cancelled.</exception>
    internal void Cancel(
        Guid cancelledByUserId,
        string? reason,
        MedicalSpecialty specialty,
        DateTime cancelledAt,
        bool isAdministrative = false
    )
    {
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.LateCancellation)
            throw new AppointmentCancellationNotAllowedException(
                DomainErrors.Appointment.CannotCancel,
                Status
            );

        if (!isAdministrative && !CanBeCancelled(specialty, cancelledAt))
            Status = AppointmentStatus.LateCancellation;
        else
            Status = AppointmentStatus.Cancelled;

        CancelledAt = cancelledAt;
        CancelledByUserId = cancelledByUserId;
        CancellationReason = reason;

        AddDomainEvent(new AppointmentCancelledEvent(this, cancelledByUserId, reason));
    }

    /// <summary>
    /// Confirms a scheduled appointment and raises an <see cref="AppointmentConfirmedEvent"/>.
    /// </summary>
    /// <exception cref="AppointmentConfirmationNotAllowedException">Thrown when the appointment is not in <see cref="AppointmentStatus.Scheduled"/> status.</exception>
    public void Confirm(DateTime confirmedAt)
    {
        if (Status is not AppointmentStatus.Scheduled)
            throw new AppointmentConfirmationNotAllowedException(
                DomainErrors.Appointment.CannotConfirm
            );

        Status = AppointmentStatus.Confirmed;
        ConfirmedAt = confirmedAt;

        AddDomainEvent(new AppointmentConfirmedEvent(this));
    }

    /// <summary>
    /// Marks the appointment as checked in by staff, meaning the patient has arrived at the clinic.
    /// </summary>
    /// <exception cref="DomainValidationException"> Thrown when the appointment status is not <see cref="AppointmentStatus.Scheduled"/>. </exception>
    public void CheckIn(DateTime checkedInAt)
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed))
            throw new DomainValidationException(DomainErrors.Appointment.CannotCheckIn);

        Status = AppointmentStatus.CheckedIn;
        CheckedInAt = checkedInAt;

        AddDomainEvent(new AppointmentCheckedInEvent(this, checkedInAt));
    }

    /// <summary>
    /// Marks the appointment as started, meaning the doctor has initiated the consultation.
    /// </summary>
    /// <exception cref="DomainValidationException"> Thrown when the appointment status is not <see cref="AppointmentStatus.CheckedIn"/>. </exception>
    public void Start(Guid initiatorDoctorId, DateTime startedAt)
    {
        if (initiatorDoctorId != DoctorId)
            throw new DomainValidationException(DomainErrors.Appointment.UnauthorizedDoctor);

        if (Status is not AppointmentStatus.CheckedIn)
            throw new DomainValidationException(DomainErrors.Appointment.CannotStart);

        Status = AppointmentStatus.InProgress;

        AddDomainEvent(new AppointmentStartedEvent(this, startedAt));
    }

    /// <summary>
    /// Marks the appointment as completed, meaning the consultation has finished.
    /// </summary>
    /// <exception cref="DomainValidationException"> Thrown when the appointment status is not <see cref="AppointmentStatus.InProgress"/>. </exception>
    public void Complete(DateTime completedAt)
    {
        if (Status is not AppointmentStatus.InProgress)
            throw new DomainValidationException(DomainErrors.Appointment.CannotComplete);

        Status = AppointmentStatus.Completed;

        AddDomainEvent(new AppointmentCompletedEvent(this, completedAt));
    }

    /// <summary>
    /// Reschedules the appointment to a new date and time range. Only one reschedule is allowed per appointment.
    /// </summary>
    /// <exception cref="AppointmentReschedulingNotAllowedException">Thrown when the appointment has already been rescheduled or is not in a reschedulable status.</exception>
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

    /// <summary>
    /// Marks the appointment as a no-show, indicating that the patient did not attend.
    /// This method is intended to be called by administrative staff.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the appointment is not in a valid status to be marked as no-show.</exception>
    public void MarkAsNoShowByStaff() => MarkAsNoShow();

    /// <summary>
    /// Marks the appointment as a no-show, indicating that the patient did not attend.
    /// This method enforces that the doctor attempting to perform the action is the doctor assigned to the appointment.
    /// </summary>
    /// <param name="initiatorDoctorId">The unique identifier of the doctor initiating the action.</param>
    /// <exception cref="AppointmentNoShowUnauthorizedException">Thrown when the initiating doctor is not the assigned doctor.</exception>
    /// <exception cref="DomainValidationException">Thrown when the appointment is not in a valid status to be marked as no-show.</exception>
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
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed))
            throw new DomainValidationException(DomainErrors.Appointment.CannotMarkNoShow);

        Status = AppointmentStatus.NoShow;

        AddDomainEvent(new AppointmentMarkedAsNoShowEvent(this));
    }

    // Business Rules (Private)
    private bool CanBeCancelled(MedicalSpecialty specialty, DateTime referenceTime) =>
        specialty.IsCancellationAllowed(ScheduledDate.Add(TimeRange.Start), referenceTime);

    private bool CanBeRescheduled() => RescheduleCount < 1 && Status is AppointmentStatus.Scheduled;
}

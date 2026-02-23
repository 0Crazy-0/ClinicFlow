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
    /// <summary>
    /// Identifier of the patient for this appointment.
    /// </summary>
    public Guid PatientId { get; init; }

    /// <summary>
    /// Identifier of the attending doctor.
    /// </summary>
    public Guid DoctorId { get; init; }

    /// <summary>
    /// Identifier of the appointment type definition.
    /// </summary>
    public Guid AppointmentTypeId { get; init; }

    /// <summary>
    /// Date on which the appointment is scheduled.
    /// </summary>
    public DateTime ScheduledDate { get; private set; }

    /// <summary>
    /// Start and end time window for the appointment.
    /// </summary>
    public TimeRange TimeRange { get; private set; }

    /// <summary>
    /// Current lifecycle status of the appointment.
    /// </summary>
    public AppointmentStatus Status { get; private set; }

    /// <summary>
    /// Optional notes provided by the patient.
    /// </summary>
    public string PatientNotes { get; private set; } = string.Empty;

    /// <summary>
    /// Optional notes added by the receptionist.
    /// </summary>
    public string ReceptionistNotes { get; private set; } = string.Empty;

    /// <summary>
    /// UTC timestamp of when the appointment was confirmed, if applicable.
    /// </summary>
    public DateTime? ConfirmedAt { get; private set; }

    /// <summary>
    /// UTC timestamp of when the appointment was cancelled, if applicable.
    /// </summary>
    public DateTime? CancelledAt { get; private set; }

    /// <summary>
    /// Reason provided for cancellation, if any.
    /// </summary>
    public string? CancellationReason { get; private set; }

    /// <summary>
    /// Identifier of the user who cancelled the appointment, if applicable.
    /// </summary>
    public Guid? CancelledByUserId { get; private set; }

    /// <summary>
    /// Number of times this appointment has been rescheduled.
    /// </summary>
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
        Status = AppointmentStatus.Scheduled;
        RescheduleCount = 0;
    }

    /// <summary>
    /// Creates a new appointment in <see cref="AppointmentStatus.Scheduled"/> status and raises an <see cref="AppointmentScheduledEvent"/>.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when any required identifier is empty or the time range is null.</exception>
    internal static Appointment Schedule(Guid patientId, Guid doctorId, Guid appointmentTypeId, DateTime scheduledDate, TimeRange timeRange)
    {
        if (patientId == Guid.Empty) throw new DomainValidationException("Patient ID cannot be empty.");
        if (doctorId == Guid.Empty) throw new DomainValidationException("Doctor ID cannot be empty.");
        if (appointmentTypeId == Guid.Empty) throw new DomainValidationException("Appointment type ID cannot be empty.");
        if (timeRange is null) throw new DomainValidationException("Time range cannot be null.");

        var appointment = new Appointment(patientId, doctorId, appointmentTypeId, scheduledDate, timeRange);

        appointment.AddDomainEvent(new AppointmentScheduledEvent(appointment));

        return appointment;
    }

    /// <summary>
    /// Cancels the appointment. If the cancellation falls within the specialty's minimum notice period,
    /// the status is set to <see cref="AppointmentStatus.LateCancellation"/> instead.
    /// </summary>
    /// <param name="specialty">The medical specialty, used to evaluate the cancellation notice policy.</param>
    /// <exception cref="AppointmentCancellationNotAllowedException">Thrown when the appointment is already cancelled.</exception>
    internal void Cancel(Guid cancelledByUserId, string? reason, MedicalSpecialty specialty)
    {
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.LateCancellation) throw new AppointmentCancellationNotAllowedException(Status);

        if (!CanBeCancelled(specialty)) Status = AppointmentStatus.LateCancellation;
        else Status = AppointmentStatus.Cancelled;

        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = cancelledByUserId;
        CancellationReason = reason;

        AddDomainEvent(new AppointmentCancelledEvent(this, cancelledByUserId, reason));
    }

    /// <summary>
    /// Confirms a scheduled appointment and raises an <see cref="AppointmentConfirmedEvent"/>.
    /// </summary>
    /// <exception cref="AppointmentConfirmationNotAllowedException">Thrown when the appointment is not in <see cref="AppointmentStatus.Scheduled"/> status.</exception>
    public void Confirm()
    {
        if (Status is not AppointmentStatus.Scheduled) throw new AppointmentConfirmationNotAllowedException("Only scheduled appointments can be confirmed");

        Status = AppointmentStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        AddDomainEvent(new AppointmentConfirmedEvent(this));
    }

    /// <summary>
    /// Reschedules the appointment to a new date and time range. Only one reschedule is allowed per appointment.
    /// </summary>
    /// <exception cref="AppointmentReschedulingNotAllowedException">Thrown when the appointment has already been rescheduled or is not in a reschedulable status.</exception>
    public void Reschedule(DateTime newDate, TimeRange newTimeRange)
    {
        if (!CanBeRescheduled()) throw new AppointmentReschedulingNotAllowedException("This appointment cannot be rescheduled");

        var previousDate = ScheduledDate;
        var previousTimeRange = TimeRange;

        ScheduledDate = newDate;
        TimeRange = newTimeRange;
        RescheduleCount++;

        AddDomainEvent(new AppointmentRescheduledEvent(this, previousDate, previousTimeRange));
    }

    // Business Rules (Private)
    private bool CanBeCancelled(MedicalSpecialty specialty) => specialty.IsCancellationAllowed(ScheduledDate.Add(TimeRange.Start));

    private bool CanBeRescheduled() => RescheduleCount < 1 && Status is AppointmentStatus.Scheduled;
}

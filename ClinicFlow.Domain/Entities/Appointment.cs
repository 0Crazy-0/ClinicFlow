using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

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
        Status = AppointmentStatus.Scheduled;
        RescheduleCount = 0;
    }

    // Factory Method
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

    // Public Domain Methods
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

    public void Confirm()
    {
        if (Status is not AppointmentStatus.Scheduled) throw new AppointmentConfirmationNotAllowedException("Only scheduled appointments can be confirmed");

        Status = AppointmentStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        AddDomainEvent(new AppointmentConfirmedEvent(this));
    }

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

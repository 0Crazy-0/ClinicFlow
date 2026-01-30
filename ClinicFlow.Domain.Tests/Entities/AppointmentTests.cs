using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentTests
{
    [Fact]
    public void Schedule_ShouldCreateAppointment_WhenValidDataProvided()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.Date.AddDays(1);
        var timeRange = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        // Act
        var appointment = Appointment.Schedule(patientId, doctorId, appointmentTypeId, scheduledDate, timeRange);

        // Assert
        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(patientId);
        appointment.DoctorId.Should().Be(doctorId);
        appointment.AppointmentTypeId.Should().Be(appointmentTypeId);
        appointment.ScheduledDate.Should().Be(scheduledDate);
        appointment.TimeRange.Should().Be(timeRange);
        appointment.Status.Should().Be(AppointmentStatusEnum.Scheduled);
        appointment.RescheduleCount.Should().Be(0);
        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentScheduledEvent);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled_WhenNoticePeriodIsSufficient()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(25));
        var userId = Guid.NewGuid();
        var reason = "Changed my mind";
        var minHours = 24;

        // Act
        appointment.Cancel(userId, reason, minHours);

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.Cancelled);
        appointment.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appointment.CancelledByUserId.Should().Be(userId);
        appointment.CancellationReason.Should().Be(reason);
        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentCancelledEvent);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToLateCancellation_WhenNoticePeriodIsInsufficient()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(2));
        var userId = Guid.NewGuid();
        var reason = "Last minute change";
        var minHours = 24;

        // Act
        appointment.Cancel(userId, reason, minHours);

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.LateCancellation);
        appointment.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        appointment.CancelledByUserId.Should().Be(userId);
        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentCancelledEvent);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenAlreadyCancelled()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        appointment.Cancel(Guid.NewGuid(), "First cancellation", 24);

        // Act
        var act = () => appointment.Cancel(Guid.NewGuid(), "Second cancellation", 24);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage($"Cannot cancel appointment. Current status: {AppointmentStatusEnum.Cancelled}");
    }

    [Fact]
    public void Confirm_ShouldSetStatusToConfirmed_WhenStatusIsScheduled()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.Confirmed);
        appointment.ConfirmedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Confirm_ShouldThrowException_WhenStatusIsNotScheduled()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));

        appointment.Cancel(Guid.NewGuid(), "Cancelled", 24);

        // Act
        var act = () => appointment.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Only scheduled appointments can be confirmed.");
    }

    [Fact]
    public void Reschedule_ShouldUpdateDateAndTime_WhenValid()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));
        var newDate = DateTime.UtcNow.Date.AddDays(2);
        var newTimeRange = new TimeRange(TimeSpan.FromHours(14), TimeSpan.FromHours(15));
        var existingAppointments = new List<Appointment>();

        // Act
        appointment.Reschedule(newDate, newTimeRange, existingAppointments);

        // Assert
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.TimeRange.Should().Be(newTimeRange);
        appointment.RescheduleCount.Should().Be(1);
        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentRescheduledEvent);
    }

    [Fact]
    public void Reschedule_ShouldThrowException_WhenAlreadyRescheduledOnce()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));

        appointment.Reschedule(DateTime.UtcNow.Date.AddDays(2), new TimeRange(TimeSpan.FromHours(14), TimeSpan.FromHours(15)), []);

        // Act
        var act = () => appointment.Reschedule(DateTime.UtcNow.Date.AddDays(3), new TimeRange(TimeSpan.FromHours(10), TimeSpan.FromHours(11)), []);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("This appointment cannot be rescheduled.");
    }

    [Fact]
    public void Reschedule_ShouldThrowException_WhenConflictExists()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));
        var newDate = DateTime.UtcNow.Date.AddDays(2);
        var newTimeRange = new TimeRange(TimeSpan.FromHours(10), TimeSpan.FromHours(11));
        var conflictingAppointment = Appointment.Schedule(Guid.NewGuid(), appointment.DoctorId, Guid.NewGuid(), newDate, newTimeRange);
        var existingAppointments = new List<Appointment> { conflictingAppointment };

        // Act
        var act = () => appointment.Reschedule(newDate, newTimeRange, existingAppointments);

        // Assert
        act.Should().Throw<AppointmentConflictException>();
    }

    private Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDateTime.Date,
        new TimeRange(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));

}

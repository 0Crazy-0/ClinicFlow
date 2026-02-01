using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentTests
{
// Schedule
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

    // Cancel
    [Fact]
    public void Cancel_ShouldSetStatusToCancelled_WhenCalledWithValidParams()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(48));
        var userId = Guid.NewGuid();

        // Act
        appointment.Cancel(userId, "Reason", 24);

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.Cancelled);
        appointment.CancelledByUserId.Should().Be(userId);

        var evt = appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Single();
        evt.Reason.Should().Be("Reason");
    }

    [Fact]
    public void Cancel_ShouldSetStatusToLateCancellation_WhenNoticePeriodIsInsufficient()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(2));
        var userId = Guid.NewGuid();

        // Act
        appointment.Cancel(userId, "Urgent", 24);

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.LateCancellation);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenAlreadyCancelled()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(48));
        var userId = Guid.NewGuid();

        appointment.Cancel(userId, "First", 24);

        // Act
        var act = () => appointment.Cancel(userId, "Second", 24);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage($"Cannot cancel appointment. Current status: {AppointmentStatusEnum.Cancelled}");
    }

    // Confirm
    [Fact]
    public void Confirm_ShouldSetStatusToConfirmed_WhenStatusIsScheduled()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.Confirmed);
    }

    [Fact]
    public void Confirm_ShouldThrowException_WhenStatusIsNotScheduled()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));
        var userId = Guid.NewGuid();
        appointment.Cancel(userId, "Cancelled", 24);

        // Act
        var act = () => appointment.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    // Reschedule
    [Fact]
    public void Reschedule_ShouldUpdateDateAndTime_WhenValid()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));
        var newDate = DateTime.UtcNow.Date.AddDays(2);
        var newTimeRange = new TimeRange(TimeSpan.FromHours(14), TimeSpan.FromHours(15));

        // Act
        appointment.Reschedule(newDate, newTimeRange, []);

        // Assert
        appointment.ScheduledDate.Should().Be(newDate);
    }

    // Helpers

    private Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDateTime.Date,
        new TimeRange(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));
}

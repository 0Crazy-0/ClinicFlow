using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Schedule_ShouldCreateAppointment_WhenValidDataProvided()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();
        var scheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1);
        var timeRange = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        // Act
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            appointmentTypeId,
            scheduledDate,
            timeRange
        );

        // Assert
        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(patientId);
        appointment.DoctorId.Should().Be(doctorId);
        appointment.AppointmentTypeId.Should().Be(appointmentTypeId);
        appointment.ScheduledDate.Should().Be(scheduledDate);
        appointment.TimeRange.Should().Be(timeRange);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.RescheduleCount.Should().Be(0);
        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentScheduledEvent);
    }

    [Theory]
    [InlineData(
        "00000000-0000-0000-0000-000000000000",
        "11111111-1111-1111-1111-111111111111",
        "22222222-2222-2222-2222-222222222222",
        DomainErrors.Validation.ValueRequired
    )]
    [InlineData(
        "11111111-1111-1111-1111-111111111111",
        "00000000-0000-0000-0000-000000000000",
        "22222222-2222-2222-2222-222222222222",
        DomainErrors.Validation.ValueRequired
    )]
    [InlineData(
        "11111111-1111-1111-1111-111111111111",
        "22222222-2222-2222-2222-222222222222",
        "00000000-0000-0000-0000-000000000000",
        DomainErrors.Validation.ValueRequired
    )]
    public void Schedule_ShouldThrowException_WhenIdIsEmpty(
        string patientIdStr,
        string doctorIdStr,
        string appointmentTypeIdStr,
        string expectedMessage
    )
    {
        // Arrange & Act
        var act = () =>
            Appointment.Schedule(
                Guid.Parse(patientIdStr),
                Guid.Parse(doctorIdStr),
                Guid.Parse(appointmentTypeIdStr),
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
                TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10))
            );

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void Schedule_ShouldThrowException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            Appointment.Schedule(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

        // Act
        appointment.Cancel(userId, "Reason", _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancelledByUserId.Should().Be(userId);
        appointment.CancellationReason.Should().Be("Reason");
        appointment.CancelledAt.Should().Be(_fakeTime.GetUtcNow().UtcDateTime);

        var evt = appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Single();
        evt.Reason.Should().Be("Reason");
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenAlreadyCancelled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

        appointment.Cancel(userId, "First", _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => appointment.Cancel(userId, "Second", _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<AppointmentCancellationNotAllowedException>()
            .Where(e => e.CurrentStatus == AppointmentStatus.Cancelled)
            .WithMessage(DomainErrors.Appointment.CannotCancel);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenAlreadyLateCancelled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

        appointment.CancelLate(userId, "Late", _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => appointment.Cancel(userId, "Second", _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<AppointmentCancellationNotAllowedException>()
            .Where(e => e.CurrentStatus == AppointmentStatus.LateCancellation)
            .WithMessage(DomainErrors.Appointment.CannotCancel);
    }

    [Fact]
    public void CancelLate_ShouldSetStatusToLateCancellation()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

        // Act
        appointment.CancelLate(userId, "Late reason", _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.LateCancellation);
        appointment.CancelledByUserId.Should().Be(userId);
        appointment.CancellationReason.Should().Be("Late reason");
        appointment.CancelledAt.Should().Be(_fakeTime.GetUtcNow().UtcDateTime);

        var evt = appointment.DomainEvents.OfType<AppointmentLateCancelledEvent>().Single();
        evt.Reason.Should().Be("Late reason");
    }

    [Fact]
    public void CancelLate_ShouldThrowException_WhenAlreadyCancelled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

        appointment.Cancel(userId, "First", _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => appointment.CancelLate(userId, "Second", _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<AppointmentCancellationNotAllowedException>()
            .Where(e => e.CurrentStatus == AppointmentStatus.Cancelled)
            .WithMessage(DomainErrors.Appointment.CannotCancel);
    }

    [Fact]
    public void CancelLate_ShouldThrowException_WhenAlreadyLateCancelled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

        appointment.CancelLate(userId, "First", _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => appointment.CancelLate(userId, "Second", _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<AppointmentCancellationNotAllowedException>()
            .Where(e => e.CurrentStatus == AppointmentStatus.LateCancellation)
            .WithMessage(DomainErrors.Appointment.CannotCancel);
    }

    [Fact]
    public void Reschedule_ShouldUpdateDateAndTime_WhenValid()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var newDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date;
        var newTimeRange = TimeRange.Create(TimeSpan.FromHours(14), TimeSpan.FromHours(15));

        // Act
        appointment.Reschedule(newDate, newTimeRange);

        // Assert
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.TimeRange.Should().Be(newTimeRange);
    }

    [Fact]
    public void CheckIn_ShouldSetStatusToCheckedIn_WhenStatusIsScheduled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        // Act
        appointment.CheckIn(_fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
        appointment.CheckedInAt.Should().Be(_fakeTime.GetUtcNow().UtcDateTime);
        appointment.DomainEvents.Should().Contain(e => e is AppointmentCheckedInEvent);
    }

    [Fact]
    public void CheckIn_ShouldThrowException_WhenStatusIsNotScheduled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.Cancel(Guid.NewGuid(), "Reason", _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => appointment.CheckIn(_fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotCheckIn);
    }

    [Fact]
    public void Start_ShouldSetStatusToInProgress_WhenValid()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.CheckIn(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        appointment.Start(appointment.DoctorId, _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
        appointment.DomainEvents.Should().Contain(e => e is AppointmentStartedEvent);
    }

    [Fact]
    public void Start_ShouldThrowException_WhenDoctorIdDiffers()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.CheckIn(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => appointment.Start(Guid.NewGuid(), _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedDoctor);
    }

    [Fact]
    public void Start_ShouldThrowException_WhenStatusIsNotCheckedIn()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        // Act
        var act = () => appointment.Start(appointment.DoctorId, _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotStart);
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompleted_WhenStatusIsInProgress()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.CheckIn(_fakeTime.GetUtcNow().UtcDateTime);
        appointment.Start(appointment.DoctorId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        appointment.Complete(_fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.DomainEvents.Should().Contain(e => e is AppointmentCompletedEvent);
    }

    [Fact]
    public void Complete_ShouldThrowException_WhenStatusIsNotInProgress()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        // Act
        var act = () => appointment.Complete(_fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotComplete);
    }

    [Fact]
    public void MarkAsRequiresReassignment_ShouldSetStatus_WhenScheduled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        // Act
        appointment.MarkAsRequiresReassignment();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.RequiresReassignment);
    }

    [Fact]
    public void MarkAsRequiresReassignment_ShouldThrowException_WhenNotScheduled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.Cancel(Guid.NewGuid(), "Reason", _fakeTime.GetUtcNow().UtcDateTime);

        // Act & Assert
        appointment
            .Invoking(a => a.MarkAsRequiresReassignment())
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotReassign);
    }

    [Fact]
    public void Reassign_ShouldUpdateDoctorAndScheduleAndEmitEvent_WhenValid()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var originalDoctorId = appointment.DoctorId;
        appointment.MarkAsRequiresReassignment();
        appointment.ClearDomainEvents();

        var newDoctorId = Guid.NewGuid();
        var newDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date;
        var newTimeRange = TimeRange.Create(TimeSpan.FromHours(14), TimeSpan.FromHours(15));

        // Act
        appointment.Reassign(newDoctorId, newDate, newTimeRange);

        // Assert
        appointment.DoctorId.Should().Be(newDoctorId);
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.TimeRange.Should().Be(newTimeRange);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);

        var evt = appointment.DomainEvents.OfType<AppointmentReassignedEvent>().Single();
        evt.PreviousDoctorId.Should().Be(originalDoctorId);
    }

    [Fact]
    public void Reassign_ShouldThrowException_WhenNewTimeRangeIsNull()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.MarkAsRequiresReassignment();

        // Act
        var act = () =>
            appointment.Reassign(
                Guid.NewGuid(),
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Reassign_ShouldThrowException_WhenNewDoctorIdIsEmpty()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.MarkAsRequiresReassignment();

        // Act
        var act = () =>
            appointment.Reassign(
                Guid.Empty,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
                TimeRange.Create(TimeSpan.FromHours(14), TimeSpan.FromHours(15))
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Reassign_ShouldThrowException_WhenNotInRequiresReassignment()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        // Act
        var act = () =>
            appointment.Reassign(
                Guid.NewGuid(),
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
                TimeRange.Create(TimeSpan.FromHours(14), TimeSpan.FromHours(15))
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotReassign);
    }

    private static Appointment CreateAppointment(DateTime scheduledDateTime) =>
        Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDateTime.Date,
            TimeRange.Create(
                scheduledDateTime.TimeOfDay,
                scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );
}

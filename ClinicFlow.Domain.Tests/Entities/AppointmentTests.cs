using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Schedule_ShouldCreateAppointment_WhenValidDataProvided()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var timeRange = TimeRange.Create(new TimeOnly(9), new TimeOnly(10));
        var scheduledByUserId = Guid.CreateVersion7();

        // Act
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            appointmentTypeId,
            scheduledDate,
            timeRange,
            scheduledByUserId
        );

        // Assert
        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(patientId);
        appointment.DoctorId.Should().Be(doctorId);
        appointment.AppointmentTypeId.Should().Be(appointmentTypeId);
        appointment.ScheduledDate.Should().Be(scheduledDate);
        appointment.TimeRange.Should().Be(timeRange);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.PatientNotes.Should().BeEmpty();
        appointment.ScheduledByUserId.Should().Be(scheduledByUserId);
        appointment.RescheduleCount.Should().Be(0);
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Schedule_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            Appointment.Schedule(
                Guid.Empty,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
                TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
                Guid.CreateVersion7()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Schedule_ShouldThrowException_WhenDoctorIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            Appointment.Schedule(
                Guid.CreateVersion7(),
                Guid.Empty,
                Guid.CreateVersion7(),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
                TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
                Guid.CreateVersion7()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Schedule_ShouldThrowException_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            Appointment.Schedule(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.Empty,
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
                TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
                Guid.CreateVersion7()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Schedule_ShouldThrowException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            Appointment.Schedule(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
                null!,
                Guid.CreateVersion7()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Schedule_ShouldThrowException_WhenScheduledByIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            Appointment.Schedule(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
                TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
                Guid.Empty
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        // Arrange
        var appointment = CreateAppointment();
        var userId = Guid.CreateVersion7();

        // Act
        appointment.Cancel(
            userId,
            "Reason",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancelledByUserId.Should().Be(userId);
        appointment.CancellationReason.Should().Be("Reason");
        appointment
            .CancelledAt.Should()
            .Be(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));
        appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenAlreadyCancelled()
    {
        // Arrange
        var appointment = CreateAppointment();
        var userId = Guid.CreateVersion7();

        appointment.Cancel(
            userId,
            "First",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        // Act
        var act = () =>
            appointment.Cancel(
                userId,
                "Second",
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            );

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
        var appointment = CreateAppointment();
        var userId = Guid.CreateVersion7();

        appointment.CancelLate(
            userId,
            "Late",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        // Act
        var act = () =>
            appointment.Cancel(
                userId,
                "Second",
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            );

        // Assert
        act.Should()
            .Throw<AppointmentCancellationNotAllowedException>()
            .Where(e => e.CurrentStatus == AppointmentStatus.LateCancellation)
            .WithMessage(DomainErrors.Appointment.CannotCancel);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenReasonExceedsMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        var reason = new string('A', Appointment.MaxCancellationReasonLength + 1);

        // Act
        var act = () =>
            appointment.Cancel(
                Guid.CreateVersion7(),
                reason,
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Cancel_ShouldSucceed_WhenReasonLengthEqualsMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        var userId = Guid.CreateVersion7();
        var reason = new string('A', Appointment.MaxCancellationReasonLength);
        var cancelledAt = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        appointment.Cancel(userId, reason, cancelledAt);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancelledByUserId.Should().Be(userId);
        appointment.CancellationReason.Should().Be(reason);
        appointment.CancelledAt.Should().Be(cancelledAt);
        appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Cancel_ShouldSucceed_WhenReasonIsNull()
    {
        // Arrange
        var appointment = CreateAppointment();
        var userId = Guid.CreateVersion7();
        var cancelledAt = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        appointment.Cancel(userId, null, cancelledAt);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancelledByUserId.Should().Be(userId);
        appointment.CancellationReason.Should().BeNull();
        appointment.CancelledAt.Should().Be(cancelledAt);
        appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CancelLate_ShouldThrowException_WhenReasonExceedsMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        var reason = new string('A', Appointment.MaxCancellationReasonLength + 1);

        // Act
        var act = () =>
            appointment.CancelLate(
                Guid.CreateVersion7(),
                reason,
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void CancelLate_ShouldSucceed_WhenReasonLengthEqualsMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        var userId = Guid.CreateVersion7();
        var reason = new string('A', Appointment.MaxCancellationReasonLength);
        var cancelledAt = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        appointment.CancelLate(userId, reason, cancelledAt);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.LateCancellation);
        appointment.CancelledByUserId.Should().Be(userId);
        appointment.CancellationReason.Should().Be(reason);
        appointment.CancelledAt.Should().Be(cancelledAt);
        appointment.DomainEvents.OfType<AppointmentLateCancelledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CancelLate_ShouldSucceed_WhenReasonIsNull()
    {
        // Arrange
        var appointment = CreateAppointment();
        var userId = Guid.CreateVersion7();
        var cancelledAt = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        appointment.CancelLate(userId, null, cancelledAt);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.LateCancellation);
        appointment.CancelledByUserId.Should().Be(userId);
        appointment.CancellationReason.Should().BeNull();
        appointment.CancelledAt.Should().Be(cancelledAt);
        appointment.DomainEvents.OfType<AppointmentLateCancelledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CancelLate_ShouldSetStatusToLateCancellation()
    {
        // Arrange
        var appointment = CreateAppointment();
        var userId = Guid.CreateVersion7();

        // Act
        appointment.CancelLate(
            userId,
            "Late reason",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.LateCancellation);
        appointment.CancelledByUserId.Should().Be(userId);
        appointment.CancellationReason.Should().Be("Late reason");
        appointment
            .CancelledAt.Should()
            .Be(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));
        appointment.DomainEvents.OfType<AppointmentLateCancelledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CancelLate_ShouldThrowException_WhenAlreadyCancelled()
    {
        // Arrange
        var appointment = CreateAppointment();
        var userId = Guid.CreateVersion7();

        appointment.Cancel(
            userId,
            "First",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        // Act
        var act = () =>
            appointment.CancelLate(
                userId,
                "Second",
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            );

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
        var appointment = CreateAppointment();
        var userId = Guid.CreateVersion7();

        appointment.CancelLate(
            userId,
            "First",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        // Act
        var act = () =>
            appointment.CancelLate(
                userId,
                "Second",
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            );

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
        var appointment = CreateAppointment();
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var newTimeRange = TimeRange.Create(new TimeOnly(14), new TimeOnly(15));

        // Act
        appointment.Reschedule(newDate, newTimeRange);

        // Assert
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.TimeRange.Should().Be(newTimeRange);
    }

    [Fact]
    public void Reschedule_ShouldThrowException_WhenAlreadyRescheduled()
    {
        // Arrange
        var appointment = CreateAppointment();
        var newDate1 = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var newTimeRange1 = TimeRange.Create(new TimeOnly(14), new TimeOnly(15));

        appointment.Reschedule(newDate1, newTimeRange1);

        var newDate2 = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));
        var newTimeRange2 = TimeRange.Create(new TimeOnly(16), new TimeOnly(17));

        // Act
        var act = () => appointment.Reschedule(newDate2, newTimeRange2);

        // Assert
        act.Should()
            .Throw<AppointmentReschedulingNotAllowedException>()
            .WithMessage(DomainErrors.Appointment.CannotReschedule);
    }

    [Fact]
    public void Reschedule_ShouldThrowException_WhenStatusIsNotScheduled()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.Cancel(
            Guid.CreateVersion7(),
            "Reason",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var newTimeRange = TimeRange.Create(new TimeOnly(14), new TimeOnly(15));

        // Act
        var act = () => appointment.Reschedule(newDate, newTimeRange);

        // Assert
        act.Should()
            .Throw<AppointmentReschedulingNotAllowedException>()
            .WithMessage(DomainErrors.Appointment.CannotReschedule);
    }

    [Fact]
    public void CheckIn_ShouldSetStatusToCheckedIn_WhenStatusIsScheduled()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(new TimeOnly(9, 30)));

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
        appointment.CheckedInAt.Should().Be(appointment.ScheduledDate);
        appointment.ReceptionistNotes.Should().BeEmpty();
        appointment.DomainEvents.OfType<AppointmentCheckedInEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CheckIn_ShouldThrowException_WhenCheckedInAtIsNotOnScheduledDate()
    {
        // Arrange
        var appointment = CreateAppointment();
        var checkedInAt = appointment.ScheduledDate.AddDays(1).ToDateTime(new TimeOnly(9, 30));

        // Act
        var act = () => appointment.CheckIn(checkedInAt);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.InvalidCheckInDate);

        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.CheckedInAt.Should().BeNull();
        appointment.ReceptionistNotes.Should().BeEmpty();
        appointment.DomainEvents.OfType<AppointmentCheckedInEvent>().Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void CheckIn_ShouldThrowException_WhenCheckedInAtIsAtOrAfterEndTime(int minutesAfterEnd)
    {
        // Arrange
        var appointment = CreateAppointment();
        var checkedInAt = appointment
            .ScheduledDate.ToDateTime(appointment.TimeRange.End)
            .AddMinutes(minutesAfterEnd);

        // Act
        var act = () => appointment.CheckIn(checkedInAt);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CheckInAfterEndTime);

        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.CheckedInAt.Should().BeNull();
        appointment.ReceptionistNotes.Should().BeEmpty();
        appointment.DomainEvents.OfType<AppointmentCheckedInEvent>().Should().BeEmpty();
    }

    [Fact]
    public void CheckIn_ShouldSetReceptionistNotesToEmpty_WhenNullProvided()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue), null);

        // Assert
        appointment.ReceptionistNotes.Should().BeEmpty();
    }

    [Fact]
    public void CheckIn_ShouldThrowException_WhenStatusIsNotScheduled()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.Cancel(
            Guid.CreateVersion7(),
            "Reason",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        // Act
        var act = () =>
            appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotCheckIn);
    }

    [Fact]
    public void CheckIn_ShouldSetReceptionistNotes_WhenProvided()
    {
        // Arrange
        var appointment = CreateAppointment();
        var receptionistNotes = "Test receptionist notes";

        // Act
        appointment.CheckIn(
            appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue),
            receptionistNotes
        );

        // Assert
        appointment.ReceptionistNotes.Should().Be(receptionistNotes);
    }

    [Fact]
    public void CheckIn_ShouldThrowException_WhenReceptionistNotesExceedMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        var receptionistNotes = new string('A', Appointment.MaxNotesLength + 1);

        // Act
        var act = () =>
            appointment.CheckIn(
                appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue),
                receptionistNotes
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);

        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.CheckedInAt.Should().BeNull();
        appointment.ReceptionistNotes.Should().BeEmpty();
        appointment.DomainEvents.OfType<AppointmentCheckedInEvent>().Should().BeEmpty();
    }

    [Fact]
    public void CheckIn_ShouldSucceed_WhenReceptionistNotesLengthEqualsMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        var receptionistNotes = new string('A', Appointment.MaxNotesLength);
        var checkedInAt = appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue);

        // Act
        appointment.CheckIn(checkedInAt, receptionistNotes);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
        appointment.CheckedInAt.Should().Be(appointment.ScheduledDate);
        appointment.ReceptionistNotes.Should().Be(receptionistNotes);
    }

    [Fact]
    public void Start_ShouldSetStatusToInProgress_WhenValid()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));

        var startedAt = appointment.ScheduledDate.ToDateTime(new TimeOnly(9, 15));

        // Act
        appointment.Start(appointment.DoctorId, startedAt);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
        appointment.StartedAt.Should().Be(startedAt);
        appointment.DomainEvents.OfType<AppointmentStartedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Start_ShouldThrowException_WhenStartedAtIsBeforeAppointmentStart()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));

        // Act
        var act = () =>
            appointment.Start(
                appointment.DoctorId,
                appointment.ScheduledDate.ToDateTime(appointment.TimeRange.Start).AddSeconds(-1)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.InvalidStartDate);

        appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
        appointment.DomainEvents.OfType<AppointmentStartedEvent>().Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void Start_ShouldThrowException_WhenStartedAtIsNotOnScheduledDate(int dayOffset)
    {
        // Arrange
        var appointment = CreateAppointment(); // ScheduledDate is 2 days from now

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));

        // Act
        var act = () =>
            appointment.Start(
                appointment.DoctorId,
                appointment.ScheduledDate.AddDays(dayOffset).ToDateTime(new TimeOnly(9, 15))
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.InvalidStartDate);

        appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
        appointment.DomainEvents.OfType<AppointmentStartedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Start_ShouldThrowException_WhenDoctorIdDiffers()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));

        // Act
        var act = () =>
            appointment.Start(
                Guid.CreateVersion7(),
                appointment.ScheduledDate.ToDateTime(appointment.TimeRange.Start)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedDoctor);
    }

    [Fact]
    public void Start_ShouldThrowException_WhenStatusIsNotCheckedIn()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        var act = () =>
            appointment.Start(
                appointment.DoctorId,
                appointment.ScheduledDate.ToDateTime(appointment.TimeRange.Start)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotStart);
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompleted_WhenStatusIsInProgress()
    {
        // Arrange
        var appointment = CreateAppointment(); // TimeRange: 9:00 - 10:00

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));
        appointment.Start(
            appointment.DoctorId,
            appointment.ScheduledDate.ToDateTime(appointment.TimeRange.Start)
        );

        // Act
        var completedAt = appointment.ScheduledDate.ToDateTime(new TimeOnly(9, 30));
        appointment.Complete(completedAt);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.CompletedAt.Should().Be(completedAt);
        appointment.DomainEvents.OfType<AppointmentCompletedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Complete_ShouldThrowException_WhenCompletedAtIsBeforeAppointmentStart()
    {
        // Arrange
        var appointment = CreateAppointment(); // TimeRange: 9:00 - 10:00

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));
        appointment.Start(
            appointment.DoctorId,
            appointment.ScheduledDate.ToDateTime(appointment.TimeRange.Start)
        );

        // Act
        var act = () =>
            appointment.Complete(appointment.ScheduledDate.ToDateTime(new TimeOnly(8, 0)));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.InvalidCompletionDate);

        appointment.Status.Should().Be(AppointmentStatus.InProgress);
    }

    [Fact]
    public void Complete_ShouldSucceed_WhenCompletedAtEqualsAppointmentStart()
    {
        // Arrange
        var appointment = CreateAppointment(); // TimeRange: 9:00 - 10:00

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));
        appointment.Start(
            appointment.DoctorId,
            appointment.ScheduledDate.ToDateTime(appointment.TimeRange.Start)
        );

        // Act
        var completedAt = appointment.ScheduledDate.ToDateTime(appointment.TimeRange.Start);
        appointment.Complete(completedAt);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.CompletedAt.Should().Be(completedAt);
        appointment.DomainEvents.OfType<AppointmentCompletedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Complete_ShouldThrowException_WhenCompletedAtIsBeforeActualStart()
    {
        // Arrange
        var appointment = CreateAppointment(); // TimeRange: 9:00 - 10:00

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));
        appointment.Start(
            appointment.DoctorId,
            appointment.ScheduledDate.ToDateTime(new TimeOnly(9, 15))
        );

        // Act
        var act = () =>
            appointment.Complete(appointment.ScheduledDate.ToDateTime(new TimeOnly(9, 0)));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CompletionBeforeActualStart);

        appointment.Status.Should().Be(AppointmentStatus.InProgress);
        appointment.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Complete_ShouldSucceed_WhenCompletedAtEqualsActualStart()
    {
        // Arrange
        var appointment = CreateAppointment(); // TimeRange: 9:00 - 10:00

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));
        var startedAt = appointment.ScheduledDate.ToDateTime(new TimeOnly(9, 15));
        appointment.Start(appointment.DoctorId, startedAt);

        // Act
        appointment.Complete(startedAt);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.CompletedAt.Should().Be(startedAt);
    }

    [Fact]
    public void Complete_ShouldThrowException_WhenStatusIsNotInProgress()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        var act = () =>
            appointment.Complete(appointment.ScheduledDate.ToDateTime(new TimeOnly(9, 30)));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotComplete);
    }

    [Fact]
    public void MarkAsRequiresReassignment_ShouldSetStatus_WhenScheduled()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        appointment.MarkAsRequiresReassignment();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.RequiresReassignment);
    }

    [Fact]
    public void MarkAsRequiresReassignment_ShouldThrowException_WhenNotScheduled()
    {
        // Arrange
        var appointment = CreateAppointment();
        appointment.Cancel(
            Guid.CreateVersion7(),
            "Reason",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

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
        var appointment = CreateAppointment();

        appointment.MarkAsRequiresReassignment();
        appointment.ClearDomainEvents();

        var newDoctorId = Guid.CreateVersion7();
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));
        var newTimeRange = TimeRange.Create(new TimeOnly(14), new TimeOnly(15));

        // Act
        appointment.Reassign(newDoctorId, newDate, newTimeRange);

        // Assert
        appointment.DoctorId.Should().Be(newDoctorId);
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.TimeRange.Should().Be(newTimeRange);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.DomainEvents.OfType<AppointmentReassignedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Reassign_ShouldThrowException_WhenNewTimeRangeIsNull()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.MarkAsRequiresReassignment();

        // Act
        var act = () =>
            appointment.Reassign(
                Guid.CreateVersion7(),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
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
        var appointment = CreateAppointment();

        appointment.MarkAsRequiresReassignment();

        // Act
        var act = () =>
            appointment.Reassign(
                Guid.Empty,
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
                TimeRange.Create(new TimeOnly(14), new TimeOnly(15))
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
        var appointment = CreateAppointment();

        // Act
        var act = () =>
            appointment.Reassign(
                Guid.CreateVersion7(),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
                TimeRange.Create(new TimeOnly(14), new TimeOnly(15))
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotReassign);
    }

    [Fact]
    public void CancelDueToSystemTimeout_ShouldCancelAndEmitEvent_WhenRequiresReassignment()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.MarkAsRequiresReassignment();
        var cancelledAt = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        appointment.CancelDueToSystemTimeout(cancelledAt);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancelledAt.Should().Be(cancelledAt);
        appointment.CancelledByUserId.Should().BeNull();
        appointment.CancellationReason.Should().Be(Appointment.SystemTimeoutCancellationReason);
        appointment.DomainEvents.OfType<AppointmentSystemCancelledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CancelDueToSystemTimeout_ShouldThrowException_WhenNotInRequiresReassignment()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        var act = () =>
            appointment.CancelDueToSystemTimeout(
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotCancel);
    }

    [Fact]
    public void UpdatePatientNotes_ShouldSucceed_WhenScheduled()
    {
        // Arrange
        var appointment = CreateAppointment();
        var newNotes = "New patient notes";

        // Act
        appointment.UpdatePatientNotes(newNotes);

        // Assert
        appointment.PatientNotes.Should().Be(newNotes);
    }

    [Fact]
    public void UpdatePatientNotes_ShouldSucceed_WhenCheckedIn()
    {
        // Arrange
        var appointment = CreateAppointment();
        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));

        var newNotes = "New patient notes";

        // Act
        appointment.UpdatePatientNotes(newNotes);

        // Assert
        appointment.PatientNotes.Should().Be(newNotes);
    }

    [Fact]
    public void UpdatePatientNotes_ShouldSetNotesToEmpty_WhenNullProvided()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.UpdatePatientNotes("Initial notes");

        // Act
        appointment.UpdatePatientNotes(null);

        // Assert
        appointment.PatientNotes.Should().BeEmpty();
    }

    [Fact]
    public void UpdatePatientNotes_ShouldThrowException_WhenInvalidStatus()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.MarkAsRequiresReassignment();

        // Act
        var act = () => appointment.UpdatePatientNotes("New notes");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotUpdateNotes);
    }

    [Fact]
    public void UpdatePatientNotes_ShouldThrowException_WhenNotesExceedMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        var notes = new string('A', Appointment.MaxNotesLength + 1);

        // Act
        var act = () => appointment.UpdatePatientNotes(notes);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void UpdatePatientNotes_ShouldSucceed_WhenNotesLengthEqualsMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        var notes = new string('A', Appointment.MaxNotesLength);

        // Act
        appointment.UpdatePatientNotes(notes);

        // Assert
        appointment.PatientNotes.Should().Be(notes);
    }

    [Fact]
    public void UpdateGuardianNotes_ShouldSetTextAndAuthor_WhenScheduled()
    {
        // Arrange
        var appointment = CreateAppointment();
        var authorId = Guid.CreateVersion7();

        // Act
        appointment.UpdateGuardianNotes("notes", authorId);

        // Assert
        appointment.GuardianNotes.Should().Be("notes");
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
        appointment.PatientNotes.Should().BeEmpty();
    }

    [Fact]
    public void UpdateGuardianNotes_ShouldThrowException_WhenInvalidStatus()
    {
        // Arrange
        var appointment = CreateAppointment();
        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));

        appointment.Start(
            appointment.DoctorId,
            appointment.ScheduledDate.ToDateTime(new TimeOnly(9, 30))
        );

        // Act
        var act = () => appointment.UpdateGuardianNotes("notes", Guid.CreateVersion7());

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotUpdateNotes);
    }

    [Fact]
    public void UpdateGuardianNotes_ShouldThrowException_WhenAuthorIsEmpty()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        var act = () => appointment.UpdateGuardianNotes("notes", Guid.Empty);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void UpdateGuardianNotes_ShouldClearTextButRetainAuthor_WhenEmptyProvidedAfterPriorNote()
    {
        // Arrange
        var appointment = CreateAppointment();
        var authorId = Guid.CreateVersion7();
        appointment.UpdateGuardianNotes("notes", authorId);

        // Act
        appointment.UpdateGuardianNotes(null, authorId);

        // Assert
        appointment.GuardianNotes.Should().BeEmpty();
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
        appointment.PatientNotes.Should().BeEmpty();
    }

    [Fact]
    public void UpdateGuardianNotes_ShouldThrowException_WhenNotesExceedMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        var notes = new string('A', Appointment.MaxNotesLength + 1);

        // Act
        var act = () => appointment.UpdateGuardianNotes(notes, Guid.CreateVersion7());

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void UpdateGuardianNotes_ShouldSucceed_WhenNotesLengthEqualsMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        var authorId = Guid.CreateVersion7();
        var notes = new string('A', Appointment.MaxNotesLength);

        // Act
        appointment.UpdateGuardianNotes(notes, authorId);

        // Assert
        appointment.GuardianNotes.Should().Be(notes);
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
    }

    [Fact]
    public void UpdateReceptionistNotes_ShouldSucceed_WhenCheckedIn()
    {
        // Arrange
        var appointment = CreateAppointment();
        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));
        var newNotes = "New receptionist notes";

        // Act
        appointment.UpdateReceptionistNotes(newNotes);

        // Assert
        appointment.ReceptionistNotes.Should().Be(newNotes);
    }

    [Fact]
    public void UpdateReceptionistNotes_ShouldSetNotesToEmpty_WhenNullProvided()
    {
        // Arrange
        var appointment = CreateAppointment();
        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));
        appointment.UpdateReceptionistNotes("Initial notes");

        // Act
        appointment.UpdateReceptionistNotes(null);

        // Assert
        appointment.ReceptionistNotes.Should().BeEmpty();
    }

    [Fact]
    public void UpdateReceptionistNotes_ShouldThrowException_WhenInvalidStatus()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        var act = () => appointment.UpdateReceptionistNotes("New notes");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotUpdateNotes);
    }

    [Fact]
    public void UpdateReceptionistNotes_ShouldThrowException_WhenNotesExceedMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));
        var notes = new string('A', Appointment.MaxNotesLength + 1);

        // Act
        var act = () => appointment.UpdateReceptionistNotes(notes);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void UpdateReceptionistNotes_ShouldSucceed_WhenNotesLengthEqualsMaximumLength()
    {
        // Arrange
        var appointment = CreateAppointment();
        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));
        var notes = new string('A', Appointment.MaxNotesLength);

        // Act
        appointment.UpdateReceptionistNotes(notes);

        // Assert
        appointment.ReceptionistNotes.Should().Be(notes);
    }

    private Appointment CreateAppointment() =>
        Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            Guid.CreateVersion7()
        );
}

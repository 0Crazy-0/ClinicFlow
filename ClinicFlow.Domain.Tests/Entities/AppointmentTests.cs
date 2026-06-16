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
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var timeRange = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));

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
        appointment.PatientNotes.Should().BeEmpty();
        appointment.RescheduleCount.Should().Be(0);
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
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
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
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
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Schedule_ShouldSetPatientNotes_WhenProvided()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var timeRange = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));
        var patientNotes = "Test patient notes";

        // Act
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            appointmentTypeId,
            scheduledDate,
            timeRange,
            patientNotes
        );

        // Assert
        appointment.PatientNotes.Should().Be(patientNotes);
    }

    [Fact]
    public void Schedule_ShouldSetPatientNotesToEmpty_WhenNullProvided()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var timeRange = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0));

        // Act
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            appointmentTypeId,
            scheduledDate,
            timeRange,
            null
        );

        // Assert
        appointment.PatientNotes.Should().BeEmpty();
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

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
    public void CancelLate_ShouldSetStatusToLateCancellation()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var userId = Guid.NewGuid();

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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var newTimeRange = TimeRange.Create(new TimeOnly(14, 0), new TimeOnly(15, 0));

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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var newDate1 = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var newTimeRange1 = TimeRange.Create(new TimeOnly(14, 0), new TimeOnly(15, 0));
        appointment.Reschedule(newDate1, newTimeRange1);

        var newDate2 = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));
        var newTimeRange2 = TimeRange.Create(new TimeOnly(16, 0), new TimeOnly(17, 0));

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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.Cancel(
            Guid.NewGuid(),
            "Reason",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2));
        var newTimeRange = TimeRange.Create(new TimeOnly(14, 0), new TimeOnly(15, 0));

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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        // Act
        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
        appointment
            .CheckedInAt.Should()
            .Be(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));
        appointment.ReceptionistNotes.Should().BeEmpty();
        appointment.DomainEvents.OfType<AppointmentCheckedInEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CheckIn_ShouldSetReceptionistNotesToEmpty_WhenNullProvided()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        // Act
        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime), null);

        // Assert
        appointment.ReceptionistNotes.Should().BeEmpty();
    }

    [Fact]
    public void CheckIn_ShouldThrowException_WhenStatusIsNotScheduled()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.Cancel(
            Guid.NewGuid(),
            "Reason",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        // Act
        var act = () =>
            appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotCheckIn);
    }

    [Fact]
    public void CheckIn_ShouldSetReceptionistNotes_WhenProvided()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var receptionistNotes = "Test receptionist notes";

        // Act
        appointment.CheckIn(
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime),
            receptionistNotes
        );

        // Assert
        appointment.ReceptionistNotes.Should().Be(receptionistNotes);
    }

    [Fact]
    public void Start_ShouldSetStatusToInProgress_WhenValid()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Act
        appointment.Start(appointment.DoctorId, _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
        appointment.DomainEvents.OfType<AppointmentStartedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Start_ShouldThrowException_WhenDoctorIdDiffers()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

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
        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));
        appointment.Start(appointment.DoctorId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        appointment.Complete(_fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.DomainEvents.OfType<AppointmentCompletedEvent>().Should().ContainSingle();
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
        appointment.Cancel(
            Guid.NewGuid(),
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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.MarkAsRequiresReassignment();
        appointment.ClearDomainEvents();

        var newDoctorId = Guid.NewGuid();
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));
        var newTimeRange = TimeRange.Create(new TimeOnly(14, 0), new TimeOnly(15, 0));

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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.MarkAsRequiresReassignment();

        // Act
        var act = () =>
            appointment.Reassign(
                Guid.NewGuid(),
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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.MarkAsRequiresReassignment();

        // Act
        var act = () =>
            appointment.Reassign(
                Guid.Empty,
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
                TimeRange.Create(new TimeOnly(14, 0), new TimeOnly(15, 0))
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
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
                TimeRange.Create(new TimeOnly(14, 0), new TimeOnly(15, 0))
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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var newNotes = "New patient notes";

        // Act
        appointment.UpdatePatientNotes(newNotes);

        // Assert
        appointment.PatientNotes.Should().Be(newNotes);
    }

    [Fact]
    public void UpdatePatientNotes_ShouldSucceed_WhenRequiresReassignment()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.MarkAsRequiresReassignment();
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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Act
        var act = () => appointment.UpdatePatientNotes("New notes");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotUpdateNotes);
    }

    [Fact]
    public void UpdateReceptionistNotes_ShouldSucceed_WhenCheckedIn()
    {
        // Arrange
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));
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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        appointment.CheckIn(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));
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
        var appointment = CreateAppointment(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        // Act
        var act = () => appointment.UpdateReceptionistNotes("New notes");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotUpdateNotes);
    }

    private static Appointment CreateAppointment(DateTime scheduledDateTime) =>
        Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(scheduledDateTime),
            TimeRange.Create(
                TimeOnly.FromDateTime(scheduledDateTime),
                TimeOnly.FromDateTime(scheduledDateTime.AddHours(1))
            )
        );
}

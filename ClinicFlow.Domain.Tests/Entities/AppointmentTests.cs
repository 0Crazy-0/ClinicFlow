using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;

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
        var timeRange = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        // Act
        var appointment = Appointment.Schedule(patientId, doctorId, appointmentTypeId, scheduledDate, timeRange);

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
    [InlineData("00000000-0000-0000-0000-000000000000", "11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "Patient ID cannot be empty.")]
    [InlineData("11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000", "22222222-2222-2222-2222-222222222222", "Doctor ID cannot be empty.")]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "00000000-0000-0000-0000-000000000000", "Appointment type ID cannot be empty.")]
    public void Schedule_ShouldThrowException_WhenIdIsEmpty(string patientIdStr, string doctorIdStr, string appointmentTypeIdStr, string expectedMessage)
    {
        // Arrange & Act
        var act = () => Appointment.Schedule(Guid.Parse(patientIdStr), Guid.Parse(doctorIdStr), Guid.Parse(appointmentTypeIdStr), DateTime.UtcNow.AddDays(1), TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10)));

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void Schedule_ShouldThrowException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1), null!);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Time range cannot be null.");
    }

    // Cancel
    [Fact]
    public void Cancel_ShouldSetStatusToCancelled_WhenCalledWithValidParams()
    {
        // Arrange - Specialty requires 24h
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));
        var userId = Guid.NewGuid();
        var specialty = CreateSpecialty(24);

        // Act
        appointment.Cancel(userId, "Reason", specialty);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancelledByUserId.Should().Be(userId);

        var evt = appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Single();
        evt.Reason.Should().Be("Reason");
    }

    [Fact]
    public void Cancel_ShouldSetStatusToLateCancellation_WhenNoticePeriodIsInsufficient()
    {
        // Arrange - Specialty requires 24h
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(2));
        var userId = Guid.NewGuid();
        var specialty = CreateSpecialty(24);

        // Act
        appointment.Cancel(userId, "Urgent", specialty);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.LateCancellation);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenAlreadyCancelled()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));
        var userId = Guid.NewGuid();
        var specialty = CreateSpecialty(24);

        appointment.Cancel(userId, "First", specialty);

        // Act
        var act = () => appointment.Cancel(userId, "Second", specialty);

        // Assert
        act.Should().Throw<AppointmentCancellationNotAllowedException>().Where(e => e.CurrentStatus == AppointmentStatus.Cancelled);
    }

    // Cancellation Policy Logic Verification

    [Theory]
    [InlineData(24, 25, AppointmentStatus.Cancelled)]
    [InlineData(24, 23, AppointmentStatus.LateCancellation)]
    [InlineData(12, 13, AppointmentStatus.Cancelled)]
    [InlineData(12, 11, AppointmentStatus.LateCancellation)]
    [InlineData(2, 3, AppointmentStatus.Cancelled)]
    [InlineData(2, 1, AppointmentStatus.LateCancellation)]
    public void Cancel_ShouldEnforceMinimumHoursPolicy(int minHours, int hoursUntilAppointment, AppointmentStatus expectedStatus)
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(hoursUntilAppointment));
        var userId = Guid.NewGuid();
        var specialty = CreateSpecialty(minHours);

        // Act
        appointment.Cancel(userId, "Test Policy", specialty);

        // Assert
        appointment.Status.Should().Be(expectedStatus);

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
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void Confirm_ShouldThrowException_WhenStatusIsNotScheduled()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));
        var specialty = CreateSpecialty(24);

        //Act 
        appointment.Cancel(Guid.NewGuid(), "Cancelled", specialty);

        //Assert
        appointment.Invoking(x => x.Confirm()).Should().Throw<AppointmentConfirmationNotAllowedException>();
    }

    // Reschedule
    [Fact]
    public void Reschedule_ShouldUpdateDateAndTime_WhenValid()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));
        var newDate = DateTime.UtcNow.Date.AddDays(2);
        var newTimeRange = TimeRange.Create(TimeSpan.FromHours(14), TimeSpan.FromHours(15));

        // Act
        appointment.Reschedule(newDate, newTimeRange);

        // Assert
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.TimeRange.Should().Be(newTimeRange);
    }

    // Helpers
    private static Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDateTime.Date,
        TimeRange.Create(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));

    private static MedicalSpecialty CreateSpecialty(int minCancellationHours) => MedicalSpecialty.Create("Test Specialty", "Description", 30, minCancellationHours);

}

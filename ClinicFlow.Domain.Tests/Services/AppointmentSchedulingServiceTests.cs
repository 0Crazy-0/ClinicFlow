using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentSchedulingServiceTests
{
    private readonly Mock<IAppointmentRepository> _repositoryMock;
    private readonly AppointmentSchedulingService _service;

    public AppointmentSchedulingServiceTests()
    {
        _repositoryMock = new Mock<IAppointmentRepository>();
        _service = new AppointmentSchedulingService(_repositoryMock.Object);
    }

    // ScheduleAppointmentAsync
    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenPatientIsBlocked()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = Doctor.Create(Guid.NewGuid(), "12345", Guid.NewGuid(), "Dr. House", 101);
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var penalties = new List<PatientPenalty> { PatientPenalty.CreateBlock(patient.Id, "Blocked", scheduledDate) };

        // Act
        var act = () => _service.ScheduleAppointmentAsync(patient, penalties, doctor, scheduledDate, new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10)), Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<PatientBlockedException>();

        _repositoryMock.Verify(x => x.HasConflictAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeRange>()), Times.Never);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenConflictExists()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = Doctor.Create(Guid.NewGuid(), "12345", Guid.NewGuid(), "Dr. House", 101);
        var penalties = new List<PatientPenalty>();
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var timeRange = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        _repositoryMock.Setup(x => x.HasConflictAsync(doctor.Id, scheduledDate, timeRange)).ReturnsAsync(true);

        // Act
        var act = () => _service.ScheduleAppointmentAsync(patient, penalties, doctor, scheduledDate, timeRange, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<AppointmentConflictException>();
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldReturnAppointment_WhenSuccess()
    {
        // Arrange
        var patient = CreatePatient();
        var doctor = Doctor.Create(Guid.NewGuid(), "12345", Guid.NewGuid(), "Dr. House", 101);
        var penalties = new List<PatientPenalty>();
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var timeRange = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(10));
        var appointmentTypeId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.HasConflictAsync(doctor.Id, scheduledDate, timeRange)).ReturnsAsync(false);

        // Act
        var result = await _service.ScheduleAppointmentAsync(patient, penalties, doctor, scheduledDate, timeRange, appointmentTypeId);

        // Assert
        result.Should().NotBeNull();
        result.PatientId.Should().Be(patient.Id);
        result.DoctorId.Should().Be(doctor.Id);
        result.Status.Should().Be(AppointmentStatusEnum.Scheduled);
    }

    // RescheduleAppointmentAsync
    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldThrowException_WhenConflictExists()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1).Date.AddHours(9));
        var newDate = DateTime.UtcNow.AddDays(2);

        // Create a conflicting appointment
        var conflictingAppointment = CreateAppointment(newDate.Date.AddHours(10));

        _repositoryMock.Setup(x => x.GetByDoctorIdAsync(appointment.DoctorId, newDate.Date)).ReturnsAsync([conflictingAppointment]);

        // Act
        var act = () => _service.RescheduleAppointmentAsync(appointment, newDate, new TimeRange(TimeSpan.FromHours(10), TimeSpan.FromHours(11)));

        // Assert
        await act.Should().ThrowAsync<AppointmentConflictException>();
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldSucceed_WhenNoConflict()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1).Date.AddHours(9));

        var newDate = DateTime.UtcNow.AddDays(2);
        var newTimeRange = new TimeRange(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        _repositoryMock.Setup(x => x.GetByDoctorIdAsync(appointment.DoctorId, newDate.Date)).ReturnsAsync([]);

        // Act
        await _service.RescheduleAppointmentAsync(appointment, newDate, newTimeRange);

        // Assert
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.TimeRange.Should().Be(newTimeRange);
        appointment.RescheduleCount.Should().Be(1);
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldThrowException_WhenReschedulingNotAllowed()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1).Date.AddHours(9));
        var newDate = DateTime.UtcNow.AddDays(2);
        var newTimeRange = new TimeRange(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        // Simulate that the appointment has already been rescheduled once
        appointment.Reschedule(DateTime.UtcNow.AddDays(1).Date.AddHours(10), newTimeRange);

        _repositoryMock.Setup(x => x.GetByDoctorIdAsync(appointment.DoctorId, newDate.Date)).ReturnsAsync([]);

        // Act
        var act = () => _service.RescheduleAppointmentAsync(appointment, newDate, newTimeRange);

        // Assert
        await act.Should().ThrowAsync<AppointmentReschedulingNotAllowedException>().WithMessage("Cannot reschedule appointment: This appointment cannot be rescheduled");
    }

    // Helpers
    private Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDateTime.Date,
        new TimeRange(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));

    private Patient CreatePatient() => Patient.Create(Guid.NewGuid(), DateTime.UtcNow.AddYears(-30), "O+", "None", "None", "Mom", "555-5555");
}

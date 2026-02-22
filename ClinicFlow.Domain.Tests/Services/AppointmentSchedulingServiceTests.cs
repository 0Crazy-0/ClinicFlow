using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentSchedulingServiceTests
{
    private readonly Mock<IAppointmentRepository> _repositoryMock;
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly AppointmentSchedulingService _sut;

    public AppointmentSchedulingServiceTests()
    {
        _repositoryMock = new Mock<IAppointmentRepository>();
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _sut = new AppointmentSchedulingService(_repositoryMock.Object, _scheduleRepositoryMock.Object, _penaltyRepositoryMock.Object);
    }

    // ScheduleAppointmentAsync
    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenPatientIsBlocked()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var penalties = new List<PatientPenalty> { PatientPenalty.CreateBlock(patientId, "Blocked", scheduledDate) };
        _penaltyRepositoryMock.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync(penalties);

        // Act
        var act = () => _sut.ScheduleAppointmentAsync(patientId, doctorId, scheduledDate, TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10)), Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<PatientBlockedException>();

        _repositoryMock.Verify(x => x.HasConflictAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeRange>()), Times.Never);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenDoctorHasNoSchedule()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.AddDays(1);

        _penaltyRepositoryMock.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync([]);
        _scheduleRepositoryMock.Setup(x => x.GetByDoctorAndDayAsync(doctorId, scheduledDate.DayOfWeek)).ReturnsAsync((Schedule?)null);

        // Act
        var act = () => _sut.ScheduleAppointmentAsync(patientId, doctorId, scheduledDate, TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10)),
            Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<DoctorNotAvailableException>();

        _repositoryMock.Verify(x => x.HasConflictAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeRange>()), Times.Never);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenOutsideDoctorSchedule()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.AddDays(1);

        var schedule = CreateSchedule(doctorId, scheduledDate.DayOfWeek, TimeRange.Create(TimeSpan.FromHours(8), TimeSpan.FromHours(16)));
        _penaltyRepositoryMock.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync([]);
        _scheduleRepositoryMock.Setup(x => x.GetByDoctorAndDayAsync(doctorId, scheduledDate.DayOfWeek)).ReturnsAsync(schedule);

        // Act
        var act = () => _sut.ScheduleAppointmentAsync(patientId, doctorId, scheduledDate,
            TimeRange.Create(TimeSpan.FromHours(17), TimeSpan.FromHours(18)), Guid.NewGuid()); // Outside working hours

        // Assert
        await act.Should().ThrowAsync<DoctorNotAvailableException>();

        _repositoryMock.Verify(x => x.HasConflictAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<TimeRange>()), Times.Never);
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldThrowException_WhenConflictExists()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var timeRange = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        _penaltyRepositoryMock.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync([]);
        SetupValidSchedule(doctorId, scheduledDate.DayOfWeek);
        _repositoryMock.Setup(x => x.HasConflictAsync(doctorId, scheduledDate, timeRange)).ReturnsAsync(true);

        // Act
        var act = () => _sut.ScheduleAppointmentAsync(patientId, doctorId, scheduledDate, timeRange, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<AppointmentConflictException>();
    }

    [Fact]
    public async Task ScheduleAppointmentAsync_ShouldReturnAppointment_WhenSuccess()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var scheduledDate = DateTime.UtcNow.AddDays(1);
        var timeRange = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10));
        var appointmentTypeId = Guid.NewGuid();

        _penaltyRepositoryMock.Setup(x => x.GetByPatientIdAsync(patientId)).ReturnsAsync([]);
        SetupValidSchedule(doctorId, scheduledDate.DayOfWeek);
        _repositoryMock.Setup(x => x.HasConflictAsync(doctorId, scheduledDate, timeRange)).ReturnsAsync(false);

        // Act
        var result = await _sut.ScheduleAppointmentAsync(patientId, doctorId, scheduledDate, timeRange, appointmentTypeId);

        // Assert
        result.Should().NotBeNull();
        result.PatientId.Should().Be(patientId);
        result.DoctorId.Should().Be(doctorId);
        result.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    // RescheduleAppointmentAsync
    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldThrowException_WhenDoctorNotAvailable()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1).Date.AddHours(9));
        var newDate = DateTime.UtcNow.AddDays(2);
        var newTimeRange = TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        _scheduleRepositoryMock.Setup(x => x.GetByDoctorAndDayAsync(appointment.DoctorId, newDate.DayOfWeek)).ReturnsAsync((Schedule?)null);

        // Act
        var act = () => _sut.RescheduleAppointmentAsync(appointment, newDate, newTimeRange);

        // Assert
        await act.Should().ThrowAsync<DoctorNotAvailableException>();

        _repositoryMock.Verify(x => x.GetByDoctorIdAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldThrowException_WhenConflictExists()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1).Date.AddHours(9));
        var newDate = DateTime.UtcNow.AddDays(2);

        // Create a conflicting appointment
        var conflictingAppointment = CreateAppointment(newDate.Date.AddHours(10));

        SetupValidSchedule(appointment.DoctorId, newDate.DayOfWeek);
        _repositoryMock.Setup(x => x.GetByDoctorIdAsync(appointment.DoctorId, newDate.Date)).ReturnsAsync([conflictingAppointment]);

        // Act
        var act = () => _sut.RescheduleAppointmentAsync(appointment, newDate, TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11)));

        // Assert
        await act.Should().ThrowAsync<AppointmentConflictException>();
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ShouldSucceed_WhenNoConflict()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1).Date.AddHours(9));

        var newDate = DateTime.UtcNow.AddDays(2);
        var newTimeRange = TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        SetupValidSchedule(appointment.DoctorId, newDate.DayOfWeek);
        _repositoryMock.Setup(x => x.GetByDoctorIdAsync(appointment.DoctorId, newDate.Date)).ReturnsAsync([]);

        // Act
        await _sut.RescheduleAppointmentAsync(appointment, newDate, newTimeRange);

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
        var newTimeRange = TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11));

        // Simulate that the appointment has already been rescheduled once
        appointment.Reschedule(DateTime.UtcNow.AddDays(1).Date.AddHours(10), newTimeRange);

        SetupValidSchedule(appointment.DoctorId, newDate.DayOfWeek);
        _repositoryMock.Setup(x => x.GetByDoctorIdAsync(appointment.DoctorId, newDate.Date)).ReturnsAsync([]);

        // Act
        var act = () => _sut.RescheduleAppointmentAsync(appointment, newDate, newTimeRange);

        // Assert
        await act.Should().ThrowAsync<AppointmentReschedulingNotAllowedException>().WithMessage("Cannot reschedule appointment: This appointment cannot be rescheduled");
    }

    // Helpers
    private static Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDateTime.Date,
        TimeRange.Create(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));

    private static Patient CreatePatient() => Patient.Create(Guid.NewGuid(), DateTime.UtcNow.AddYears(-30), BloodType.Create("O+"), "None", "None",
        EmergencyContact.Create("Mom", "555-5555"));

    private static Schedule CreateSchedule(Guid doctorId, DayOfWeek dayOfWeek, TimeRange timeRange) => Schedule.Create(doctorId, dayOfWeek, timeRange);

    private void SetupValidSchedule(Guid doctorId, DayOfWeek dayOfWeek)
    {
        var schedule = CreateSchedule(doctorId, dayOfWeek, TimeRange.Create(TimeSpan.FromHours(8), TimeSpan.FromHours(18)));
        _scheduleRepositoryMock.Setup(x => x.GetByDoctorAndDayAsync(doctorId, dayOfWeek)).ReturnsAsync(schedule);
    }
}



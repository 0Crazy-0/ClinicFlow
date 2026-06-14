using System.Globalization;
using AwesomeAssertions;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Schedules.EventHandlers;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Schedules;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Schedules.EventHandlers;

public class ScheduleDeactivatedEventHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly ScheduleDeactivatedEventHandler _sut;

    public ScheduleDeactivatedEventHandlerTests()
    {
        _fakeTime = new FakeTimeProvider();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ScheduleDeactivatedEventHandler(
            _fakeTime,
            _appointmentRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldMarkAppointmentsAsRequiresReassignment_WhenNoReplacementScheduleExists()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2024-01-01T00:00:00Z", CultureInfo.InvariantCulture)
        ); // Monday

        var doctorId = Guid.NewGuid();
        var nextMondayDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(7).Date;
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            nextMondayDate,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10))
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([appointment]);

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetByDoctorAndDayAsync(doctorId, DayOfWeek.Monday, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Schedule?)null);

        var domainEvent = new ScheduleDeactivatedEvent(Guid.NewGuid(), doctorId, DayOfWeek.Monday);
        var notification = new DomainEventNotification<ScheduleDeactivatedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        appointment.Status.Should().Be(AppointmentStatus.RequiresReassignment);
    }

    [Fact]
    public async Task Handle_ShouldMarkAppointmentsAsRequiresReassignment_WhenAppointmentFallsOutsideNewSchedule()
    {
        // Arrange
        _fakeTime.SetUtcNow(DateTimeOffset.Parse("2024-01-01T00:00:00Z")); // Monday

        var doctorId = Guid.NewGuid();
        var mondayDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(7).Date;
        var appointmentOutside = Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            mondayDate,
            TimeRange.Create(TimeSpan.FromHours(15), TimeSpan.FromHours(16))
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([appointmentOutside]);

        var newSchedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(14))
        );

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetByDoctorAndDayAsync(doctorId, DayOfWeek.Monday, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(newSchedule);

        var domainEvent = new ScheduleDeactivatedEvent(Guid.NewGuid(), doctorId, DayOfWeek.Monday);
        var notification = new DomainEventNotification<ScheduleDeactivatedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        appointmentOutside.Status.Should().Be(AppointmentStatus.RequiresReassignment);
    }

    [Fact]
    public async Task Handle_ShouldNotMarkAppointment_WhenAppointmentFallsInsideNewSchedule()
    {
        // Arrange
        _fakeTime.SetUtcNow(DateTimeOffset.Parse("2024-01-01T00:00:00Z")); // Monday

        var doctorId = Guid.NewGuid();
        var mondayDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(7).Date;
        var appointmentInside = Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            mondayDate,
            TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(11))
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([appointmentInside]);

        var newSchedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(14))
        );

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetByDoctorAndDayAsync(doctorId, DayOfWeek.Monday, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(newSchedule);

        var domainEvent = new ScheduleDeactivatedEvent(Guid.NewGuid(), doctorId, DayOfWeek.Monday);
        var notification = new DomainEventNotification<ScheduleDeactivatedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        appointmentInside.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public async Task Handle_ShouldNotMarkAppointment_WhenAppointmentIsOnDifferentDayOfWeek()
    {
        // Arrange
        _fakeTime.SetUtcNow(DateTimeOffset.Parse("2024-01-01T00:00:00Z")); // Monday

        var doctorId = Guid.NewGuid();
        var tuesdayDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date;
        var tuesdayAppointment = Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            tuesdayDate,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10))
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([tuesdayAppointment]);

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetByDoctorAndDayAsync(doctorId, DayOfWeek.Monday, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Schedule?)null);

        var domainEvent = new ScheduleDeactivatedEvent(Guid.NewGuid(), doctorId, DayOfWeek.Monday);
        var notification = new DomainEventNotification<ScheduleDeactivatedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        tuesdayAppointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenDoctorHasNoFutureAppointments()
    {
        // Arrange
        var doctorId = Guid.NewGuid();

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        var domainEvent = new ScheduleDeactivatedEvent(Guid.NewGuid(), doctorId, DayOfWeek.Monday);
        var notification = new DomainEventNotification<ScheduleDeactivatedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

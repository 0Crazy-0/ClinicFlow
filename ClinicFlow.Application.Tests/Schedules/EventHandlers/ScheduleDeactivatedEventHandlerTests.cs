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
        var nextMondayDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(7));
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            nextMondayDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateOnly>(),
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
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

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
        var mondayDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(7));
        var appointmentOutside = Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            mondayDate,
            TimeRange.Create(new TimeOnly(15, 0), new TimeOnly(16, 0))
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([appointmentOutside]);

        var newSchedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(14, 0))
        );

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetByDoctorAndDayAsync(doctorId, DayOfWeek.Monday, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(newSchedule);

        var domainEvent = new ScheduleDeactivatedEvent(Guid.NewGuid(), doctorId, DayOfWeek.Monday);
        var notification = new DomainEventNotification<ScheduleDeactivatedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

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
        var mondayDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(7));
        var appointmentInside = Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            mondayDate,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([appointmentInside]);

        var newSchedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(14, 0))
        );

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetByDoctorAndDayAsync(doctorId, DayOfWeek.Monday, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(newSchedule);

        var domainEvent = new ScheduleDeactivatedEvent(Guid.NewGuid(), doctorId, DayOfWeek.Monday);
        var notification = new DomainEventNotification<ScheduleDeactivatedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        appointmentInside.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public async Task Handle_ShouldNotMarkAppointment_WhenAppointmentIsOnDifferentDayOfWeek()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2024-01-01T00:00:00Z", CultureInfo.InvariantCulture)
        ); // Monday

        var doctorId = Guid.NewGuid();
        var tuesdayDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var tuesdayAppointment = Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            tuesdayDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateOnly>(),
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
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

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
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        var domainEvent = new ScheduleDeactivatedEvent(Guid.NewGuid(), doctorId, DayOfWeek.Monday);
        var notification = new DomainEventNotification<ScheduleDeactivatedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Doctors.EventHandlers;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Doctors;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Doctors.EventHandlers;

public class DeactivateDoctorSchedulesEventHandlerTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly DeactivateDoctorSchedulesEventHandler _sut;

    public DeactivateDoctorSchedulesEventHandlerTests()
    {
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _sut = new DeactivateDoctorSchedulesEventHandler(_scheduleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeactivateAllActiveSchedules_WhenDoctorIsSuspended()
    {
        // Arrange
        var doctorId = Guid.NewGuid();

        var activeSchedule1 = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(17))
        );

        var activeSchedule2 = Schedule.Create(
            doctorId,
            DayOfWeek.Tuesday,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(17))
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetActiveByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([activeSchedule1, activeSchedule2]);

        var domainEvent = new DoctorSuspendedEvent(doctorId);
        var notification = new DomainEventNotification<DoctorSuspendedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        activeSchedule1.IsActive.Should().BeFalse();
        activeSchedule2.IsActive.Should().BeFalse();

        _scheduleRepositoryMock.Verify(
            x => x.GetActiveByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldNotDeactivateAnySchedule_WhenDoctorHasNoActiveSchedules()
    {
        // Arrange
        var doctorId = Guid.NewGuid();

        _scheduleRepositoryMock
            .Setup(x => x.GetActiveByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var domainEvent = new DoctorSuspendedEvent(doctorId);
        var notification = new DomainEventNotification<DoctorSuspendedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _scheduleRepositoryMock.Verify(
            x => x.GetActiveByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}

using AwesomeAssertions;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Doctors.EventHandlers;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Doctors;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Doctors.EventHandlers;

public class DeactivateDoctorSchedulesEventHandlerTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateDoctorSchedulesEventHandler _sut;

    public DeactivateDoctorSchedulesEventHandlerTests()
    {
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new DeactivateDoctorSchedulesEventHandler(
            _scheduleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldDeactivateAllActiveSchedules_WhenDoctorIsSuspended()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();

        var activeSchedule1 = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var activeSchedule2 = Schedule.Create(
            doctorId,
            DayOfWeek.Tuesday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetActiveByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([activeSchedule1, activeSchedule2]);

        var domainEvent = new DoctorSuspendedEvent(doctorId);
        var notification = new DomainEventNotification<DoctorSuspendedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        activeSchedule1.IsActive.Should().BeFalse();
        activeSchedule2.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenDoctorHasNoActiveSchedules()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();

        _scheduleRepositoryMock
            .Setup(x => x.GetActiveByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var domainEvent = new DoctorSuspendedEvent(doctorId);
        var notification = new DomainEventNotification<DoctorSuspendedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

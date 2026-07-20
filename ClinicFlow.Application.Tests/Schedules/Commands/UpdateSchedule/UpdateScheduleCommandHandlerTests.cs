using AwesomeAssertions;
using ClinicFlow.Application.Schedules.Commands.UpdateSchedule;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Schedules.Commands.UpdateSchedule;

public class UpdateScheduleCommandHandlerTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateScheduleCommandHandler _sut;

    public UpdateScheduleCommandHandlerTests()
    {
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new UpdateScheduleCommandHandler(
            _scheduleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldDeactivateOldAndCreateNewSchedule_WhenActiveScheduleExists()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var existingSchedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var command = new UpdateScheduleCommand(
            doctorId,
            DayOfWeek.Monday,
            new TimeOnly(10, 0),
            new TimeOnly(14, 0)
        );

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetActiveByDoctorAndDayAsync(
                    doctorId,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(existingSchedule);

        Schedule? capturedSchedule = null;
        _scheduleRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Schedule>(), It.IsAny<CancellationToken>()))
            .Callback<Schedule, CancellationToken>((s, _) => capturedSchedule = s);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeEmpty();
        existingSchedule.IsActive.Should().BeFalse();
        capturedSchedule.Should().NotBeNull();
        capturedSchedule.DoctorId.Should().Be(doctorId);
        capturedSchedule.DayOfWeek.Should().Be(DayOfWeek.Monday);
        capturedSchedule.TimeRange.Start.Should().Be(command.StartTime);
        capturedSchedule.TimeRange.End.Should().Be(command.EndTime);
        capturedSchedule.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenActiveScheduleExists()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var existingSchedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var command = new UpdateScheduleCommand(
            doctorId,
            DayOfWeek.Monday,
            new TimeOnly(10, 0),
            new TimeOnly(14, 0)
        );

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetActiveByDoctorAndDayAsync(
                    doctorId,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(existingSchedule);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _scheduleRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Schedule>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenNoActiveScheduleExists()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var command = new UpdateScheduleCommand(
            doctorId,
            DayOfWeek.Monday,
            new TimeOnly(10, 0),
            new TimeOnly(14, 0)
        );

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetActiveByDoctorAndDayAsync(
                    doctorId,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Schedule?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Schedule));

        _scheduleRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Schedule>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

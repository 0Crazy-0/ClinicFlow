using ClinicFlow.Application.Schedules.Commands.SetupWeeklySchedule;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using ScheduleSlot = (
    System.DayOfWeek DayOfWeek,
    System.TimeSpan StartTime,
    System.TimeSpan EndTime
);

namespace ClinicFlow.Application.Tests.Schedules.Commands.SetupWeeklySchedule;

public class SetupWeeklyScheduleCommandHandlerTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SetupWeeklyScheduleCommandHandler _sut;

    public SetupWeeklyScheduleCommandHandlerTests()
    {
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new SetupWeeklyScheduleCommandHandler(
            _scheduleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateAllSchedules_WhenNoDuplicatesExist()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var command = new SetupWeeklyScheduleCommand(
            doctorId,
            [
                new ScheduleSlot(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(13)),
                new ScheduleSlot(
                    DayOfWeek.Wednesday,
                    TimeSpan.FromHours(8),
                    TimeSpan.FromHours(13)
                ),
                new ScheduleSlot(DayOfWeek.Friday, TimeSpan.FromHours(14), TimeSpan.FromHours(18)),
            ]
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(id => id.Should().NotBeEmpty());

        _scheduleRepositoryMock.Verify(
            x =>
                x.CreateRangeAsync(
                    It.IsAny<IReadOnlyList<Schedule>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDuplicateDayExistsInDatabase()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var command = new SetupWeeklyScheduleCommand(
            doctorId,
            [
                new ScheduleSlot(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(13)),
                new ScheduleSlot(
                    DayOfWeek.Wednesday,
                    TimeSpan.FromHours(8),
                    TimeSpan.FromHours(13)
                ),
            ]
        );

        var existingSchedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(17))
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingSchedule]);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ScheduleAlreadyExistsException>();

        _scheduleRepositoryMock.Verify(
            x => x.CreateRangeAsync(It.IsAny<List<Schedule>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

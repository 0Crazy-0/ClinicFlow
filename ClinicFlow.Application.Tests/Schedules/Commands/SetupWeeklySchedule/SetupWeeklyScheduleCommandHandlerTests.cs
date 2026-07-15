using AwesomeAssertions;
using ClinicFlow.Application.Schedules.Commands.SetupWeeklySchedule;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

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
        var doctorId = Guid.CreateVersion7();
        var command = new SetupWeeklyScheduleCommand(
            doctorId,
            [
                new ScheduleSlot(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
                new ScheduleSlot(DayOfWeek.Wednesday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
                new ScheduleSlot(DayOfWeek.Friday, new TimeOnly(14, 0), new TimeOnly(18, 0)),
            ]
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        IReadOnlyList<Schedule>? capturedSchedules = null;
        _scheduleRepositoryMock
            .Setup(x =>
                x.CreateRangeAsync(
                    It.IsAny<IReadOnlyList<Schedule>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<IReadOnlyList<Schedule>, CancellationToken>((s, _) => capturedSchedules = s);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        capturedSchedules.Should().NotBeNull();

        var expectedSchedules = command.Slots.Select(slot => new
        {
            DoctorId = doctorId,
            slot.DayOfWeek,
        });

        capturedSchedules
            .Select(s => new { s.DoctorId, s.DayOfWeek })
            .Should()
            .BeEquivalentTo(expectedSchedules);

        result.Should().BeEquivalentTo(capturedSchedules.Select(s => s.Id));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateRangeAndSaveChanges_WhenNoDuplicatesExist()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var command = new SetupWeeklyScheduleCommand(
            doctorId,
            [
                new ScheduleSlot(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
                new ScheduleSlot(DayOfWeek.Wednesday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
                new ScheduleSlot(DayOfWeek.Friday, new TimeOnly(14, 0), new TimeOnly(18, 0)),
            ]
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
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
        var doctorId = Guid.CreateVersion7();
        var command = new SetupWeeklyScheduleCommand(
            doctorId,
            [
                new ScheduleSlot(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
                new ScheduleSlot(DayOfWeek.Wednesday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
            ]
        );

        var existingSchedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingSchedule]);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<ScheduleAlreadyExistsException>();

        _scheduleRepositoryMock.Verify(
            x => x.CreateRangeAsync(It.IsAny<List<Schedule>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

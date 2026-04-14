using ClinicFlow.Application.Schedules.Commands.CreateSchedule;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Schedules.Commands.CreateSchedule;

public class CreateScheduleCommandHandlerTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateScheduleCommandHandler _sut;

    public CreateScheduleCommandHandlerTests()
    {
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new CreateScheduleCommandHandler(
            _scheduleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateSchedule_WhenNoDuplicateExists()
    {
        // Arrange
        var command = new CreateScheduleCommand(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(17)
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        Schedule? capturedSchedule = null;
        _scheduleRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Schedule>(), It.IsAny<CancellationToken>()))
            .Callback<Schedule, CancellationToken>((s, _) => capturedSchedule = s)
            .ReturnsAsync((Schedule s, CancellationToken _) => s);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedSchedule.Should().NotBeNull();
        capturedSchedule.DoctorId.Should().Be(command.DoctorId);
        capturedSchedule.DayOfWeek.Should().Be(command.DayOfWeek);
        capturedSchedule.TimeRange.Start.Should().Be(command.StartTime);
        capturedSchedule.TimeRange.End.Should().Be(command.EndTime);
        capturedSchedule.IsActive.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenDuplicateDayExists()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var command = new CreateScheduleCommand(
            doctorId,
            DayOfWeek.Monday,
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(17)
        );

        var existingSchedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(TimeSpan.FromHours(8), TimeSpan.FromHours(13))
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingSchedule]);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<ScheduleAlreadyExistsException>()
            .WithMessage(DomainErrors.Schedule.ScheduleAlreadyExists);
        exceptionAssertion.Which.DoctorId.Should().Be(doctorId);
        exceptionAssertion.Which.DayOfWeek.Should().Be(DayOfWeek.Monday);

        _scheduleRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Schedule>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

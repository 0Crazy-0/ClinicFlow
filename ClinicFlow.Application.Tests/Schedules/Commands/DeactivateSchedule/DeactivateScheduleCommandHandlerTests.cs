using ClinicFlow.Application.Schedules.Commands.DeactivateSchedule;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Schedules.Commands.DeactivateSchedule;

public class DeactivateScheduleCommandHandlerTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeactivateScheduleCommandHandler _sut;

    public DeactivateScheduleCommandHandlerTests()
    {
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new DeactivateScheduleCommandHandler(
            _scheduleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldDeactivateSchedule_WhenScheduleExists()
    {
        // Arrange
        var schedule = Schedule.Create(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(17))
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetByIdAsync(schedule.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        var command = new DeactivateScheduleCommand(schedule.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        schedule.IsActive.Should().BeFalse();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenScheduleDoesNotExist()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        _scheduleRepositoryMock
            .Setup(x => x.GetByIdAsync(scheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);

        var command = new DeactivateScheduleCommand(scheduleId);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Schedule));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

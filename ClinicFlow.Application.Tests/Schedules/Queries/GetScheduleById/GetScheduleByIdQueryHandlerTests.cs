using AwesomeAssertions;
using ClinicFlow.Application.Schedules.Queries.DTOs;
using ClinicFlow.Application.Schedules.Queries.GetScheduleById;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Schedules.Queries.GetScheduleById;

public class GetScheduleByIdQueryHandlerTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly GetScheduleByIdQueryHandler _sut;

    public GetScheduleByIdQueryHandlerTests()
    {
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _sut = new GetScheduleByIdQueryHandler(_scheduleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnScheduleDto_WhenScheduleExists()
    {
        // Arrange
        var scheduleId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var schedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(8, 0), new TimeOnly(13, 0))
        );
        schedule.SetId(scheduleId);

        _scheduleRepositoryMock
            .Setup(x => x.GetByIdAsync(scheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        var query = new GetScheduleByIdQuery(scheduleId);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new ScheduleDto(
            schedule.Id,
            schedule.DoctorId,
            schedule.DayOfWeek,
            schedule.TimeRange.Start,
            schedule.TimeRange.End,
            schedule.IsActive
        );

        result.Should().BeEquivalentTo(expectedDto);

        _scheduleRepositoryMock.Verify(
            x => x.GetByIdAsync(scheduleId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenScheduleDoesNotExist()
    {
        // Arrange
        var query = new GetScheduleByIdQuery(Guid.CreateVersion7());

        _scheduleRepositoryMock
            .Setup(x => x.GetByIdAsync(query.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Schedule));

        _scheduleRepositoryMock.Verify(
            x => x.GetByIdAsync(query.ScheduleId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}

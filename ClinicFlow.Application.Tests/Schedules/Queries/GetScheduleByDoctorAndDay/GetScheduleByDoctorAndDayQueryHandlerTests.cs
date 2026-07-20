using AwesomeAssertions;
using ClinicFlow.Application.Schedules.Queries.DTOs;
using ClinicFlow.Application.Schedules.Queries.GetScheduleByDoctorAndDay;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Schedules.Queries.GetScheduleByDoctorAndDay;

public class GetScheduleByDoctorAndDayQueryHandlerTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly GetScheduleByDoctorAndDayQueryHandler _sut;

    public GetScheduleByDoctorAndDayQueryHandlerTests()
    {
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _sut = new GetScheduleByDoctorAndDayQueryHandler(_scheduleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnScheduleDto_WhenScheduleExists()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var dayOfWeek = DayOfWeek.Tuesday;
        var schedule = Schedule.Create(
            doctorId,
            dayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(14, 0))
        );

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetActiveByDoctorAndDayAsync(doctorId, dayOfWeek, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(schedule);

        var query = new GetScheduleByDoctorAndDayQuery(doctorId, dayOfWeek);

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
            x => x.GetActiveByDoctorAndDayAsync(doctorId, dayOfWeek, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNoScheduleExists()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var dayOfWeek = DayOfWeek.Saturday;

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetActiveByDoctorAndDayAsync(doctorId, dayOfWeek, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Schedule?)null);

        var query = new GetScheduleByDoctorAndDayQuery(doctorId, dayOfWeek);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();

        _scheduleRepositoryMock.Verify(
            x => x.GetActiveByDoctorAndDayAsync(doctorId, dayOfWeek, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}

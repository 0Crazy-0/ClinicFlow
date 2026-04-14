using ClinicFlow.Application.Schedules.Queries.GetScheduleByDoctorAndDay;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
        var doctorId = Guid.NewGuid();
        var dayOfWeek = DayOfWeek.Tuesday;
        var schedule = Schedule.Create(
            doctorId,
            dayOfWeek,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(14))
        );

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetByDoctorAndDayAsync(doctorId, dayOfWeek, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(schedule);

        var query = new GetScheduleByDoctorAndDayQuery(doctorId, dayOfWeek);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.DoctorId.Should().Be(doctorId);
        result.DayOfWeek.Should().Be(DayOfWeek.Tuesday);
        result.StartTime.Should().Be(TimeSpan.FromHours(9));
        result.EndTime.Should().Be(TimeSpan.FromHours(14));
        result.IsActive.Should().BeTrue();

        _scheduleRepositoryMock.Verify(
            x => x.GetByDoctorAndDayAsync(doctorId, dayOfWeek, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNoScheduleExists()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var dayOfWeek = DayOfWeek.Saturday;

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetByDoctorAndDayAsync(doctorId, dayOfWeek, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((Schedule?)null);

        var query = new GetScheduleByDoctorAndDayQuery(doctorId, dayOfWeek);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

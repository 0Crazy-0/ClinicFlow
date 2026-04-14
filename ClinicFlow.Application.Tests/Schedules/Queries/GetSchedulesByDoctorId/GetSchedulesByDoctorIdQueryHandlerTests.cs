using ClinicFlow.Application.Schedules.Queries.GetSchedulesByDoctorId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Schedules.Queries.GetSchedulesByDoctorId;

public class GetSchedulesByDoctorIdQueryHandlerTests
{
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly GetSchedulesByDoctorIdQueryHandler _sut;

    public GetSchedulesByDoctorIdQueryHandlerTests()
    {
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _sut = new GetSchedulesByDoctorIdQueryHandler(_scheduleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSchedules_WhenSchedulesExist()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var schedule1 = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(TimeSpan.FromHours(8), TimeSpan.FromHours(13))
        );
        var schedule2 = Schedule.Create(
            doctorId,
            DayOfWeek.Wednesday,
            TimeRange.Create(TimeSpan.FromHours(14), TimeSpan.FromHours(18))
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([schedule1, schedule2]);

        var query = new GetSchedulesByDoctorIdQuery(doctorId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].DoctorId.Should().Be(doctorId);
        resultList[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
        resultList[0].StartTime.Should().Be(TimeSpan.FromHours(8));
        resultList[0].EndTime.Should().Be(TimeSpan.FromHours(13));
        resultList[0].IsActive.Should().BeTrue();

        resultList[1].DayOfWeek.Should().Be(DayOfWeek.Wednesday);
        resultList[1].StartTime.Should().Be(TimeSpan.FromHours(14));
        resultList[1].EndTime.Should().Be(TimeSpan.FromHours(18));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoSchedulesExist()
    {
        // Arrange
        var doctorId = Guid.NewGuid();

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetSchedulesByDoctorIdQuery(doctorId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

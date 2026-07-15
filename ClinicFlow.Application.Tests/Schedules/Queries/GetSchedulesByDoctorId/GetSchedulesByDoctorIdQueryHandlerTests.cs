using AwesomeAssertions;
using ClinicFlow.Application.Schedules.Queries.DTOs;
using ClinicFlow.Application.Schedules.Queries.GetSchedulesByDoctorId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
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
        var doctorId = Guid.CreateVersion7();
        var schedule1 = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(8, 0), new TimeOnly(13, 0))
        );
        var schedule2 = Schedule.Create(
            doctorId,
            DayOfWeek.Wednesday,
            TimeRange.Create(new TimeOnly(14, 0), new TimeOnly(18, 0))
        );

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([schedule1, schedule2]);

        var query = new GetSchedulesByDoctorIdQuery(doctorId);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = new List<Schedule> { schedule1, schedule2 }.Select(
            schedule => new ScheduleDto(
                schedule.Id,
                schedule.DoctorId,
                schedule.DayOfWeek,
                schedule.TimeRange.Start,
                schedule.TimeRange.End,
                schedule.IsActive
            )
        );

        result.Should().BeEquivalentTo(expectedDtos);

        _scheduleRepositoryMock.Verify(
            x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoSchedulesExist()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();

        _scheduleRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetSchedulesByDoctorIdQuery(doctorId);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();

        _scheduleRepositoryMock.Verify(
            x => x.GetByDoctorIdAsync(doctorId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}

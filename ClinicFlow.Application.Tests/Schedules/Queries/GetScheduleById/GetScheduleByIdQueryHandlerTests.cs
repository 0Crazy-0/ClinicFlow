using ClinicFlow.Application.Schedules.Queries.GetScheduleById;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
        var scheduleId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var schedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(TimeSpan.FromHours(8), TimeSpan.FromHours(13))
        );
        schedule.SetId(scheduleId);

        _scheduleRepositoryMock
            .Setup(x => x.GetByIdAsync(scheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        var query = new GetScheduleByIdQuery(scheduleId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(scheduleId);
        result.DoctorId.Should().Be(doctorId);
        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.StartTime.Should().Be(TimeSpan.FromHours(8));
        result.EndTime.Should().Be(TimeSpan.FromHours(13));
        result.IsActive.Should().BeTrue();

        _scheduleRepositoryMock.Verify(
            x => x.GetByIdAsync(scheduleId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenScheduleDoesNotExist()
    {
        // Arrange
        var query = new GetScheduleByIdQuery(Guid.NewGuid());

        _scheduleRepositoryMock
            .Setup(x => x.GetByIdAsync(query.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);

        // Act
        var act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Schedule));
    }
}

using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByDateRange;

public class GetAppointmentsByDateRangeQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly GetAppointmentsByDateRangeQueryHandler _sut;

    public GetAppointmentsByDateRangeQueryHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _sut = new GetAppointmentsByDateRangeQueryHandler(_appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenAppointmentsExistInDateRange()
    {
        // Arrange
        var startDate = _fakeTime.GetUtcNow().UtcDateTime.Date;
        var endDate = startDate.AddDays(7);
        var query = new GetAppointmentsByDateRangeQuery(startDate, endDate, 1, 10);
        var appointments = new List<Appointment>
        {
            CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), startDate.AddDays(1)),
            CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), startDate.AddDays(3)),
        };

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByDateRangePaginatedAsync(
                    startDate,
                    endDate,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((appointments, 2));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);
        result.Items.Should().HaveCount(2);
        result
            .Items.Select(x => x.ScheduledDate)
            .Should()
            .OnlyContain(d => d >= startDate && d <= endDate);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoAppointmentsInDateRange()
    {
        // Arrange
        var startDate = _fakeTime.GetUtcNow().UtcDateTime.Date;
        var endDate = startDate.AddDays(7);
        var query = new GetAppointmentsByDateRangeQuery(startDate, endDate, 1, 10);

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByDateRangePaginatedAsync(
                    startDate,
                    endDate,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<Appointment>(), 0));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    private static Appointment CreateAppointment(
        Guid patientId,
        Guid doctorId,
        DateTime scheduledDate
    ) =>
        Appointment.Schedule(
            patientId,
            doctorId,
            Guid.NewGuid(),
            scheduledDate,
            TimeRange.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0))
        );
}

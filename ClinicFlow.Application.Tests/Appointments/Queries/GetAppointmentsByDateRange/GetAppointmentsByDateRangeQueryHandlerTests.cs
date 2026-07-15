using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
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
        var startDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var endDate = startDate.AddDays(7);
        var query = new GetAppointmentsByDateRangeQuery(startDate, endDate, 1, 10);
        var appointments = new List<Appointment>
        {
            CreateAppointment(startDate.AddDays(1)),
            CreateAppointment(startDate.AddDays(3)),
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
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = appointments.Select(a => new AppointmentDto(
            a.Id,
            a.PatientId,
            a.DoctorId,
            a.AppointmentTypeId,
            a.ScheduledDate,
            a.TimeRange.Start,
            a.TimeRange.End,
            a.Status,
            a.PatientNotes,
            a.ReceptionistNotes
        ));

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _appointmentRepositoryMock.Verify(
            x =>
                x.GetByDateRangePaginatedAsync(
                    startDate,
                    endDate,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoAppointmentsInDateRange()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
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
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(0);

        _appointmentRepositoryMock.Verify(
            x =>
                x.GetByDateRangePaginatedAsync(
                    startDate,
                    endDate,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private static Appointment CreateAppointment(DateOnly scheduledDate) =>
        Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            scheduledDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
}

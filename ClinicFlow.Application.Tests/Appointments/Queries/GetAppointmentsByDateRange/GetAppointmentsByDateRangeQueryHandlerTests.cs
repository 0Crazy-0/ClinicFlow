using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByDateRange;

public class GetAppointmentsByDateRangeQueryHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly GetAppointmentsByDateRangeQueryHandler _sut;

    public GetAppointmentsByDateRangeQueryHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _sut = new GetAppointmentsByDateRangeQueryHandler(_appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAppointmentsInDateRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(7);
        var query = new GetAppointmentsByDateRangeQuery(startDate, endDate);

        _appointmentRepositoryMock
            .Setup(x => x.GetByDateRangeAsync(query.StartDate, query.EndDate))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _appointmentRepositoryMock.Verify(
            x => x.GetByDateRangeAsync(query.StartDate, query.EndDate),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnAppointmentList_WhenAppointmentsExistInDateRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(7);
        var query = new GetAppointmentsByDateRangeQuery(startDate, endDate);

        var appointments = new List<Appointment>
        {
            CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), startDate.AddDays(1)),
            CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), startDate.AddDays(3)),
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(appointments);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<AppointmentDto>();
        result
            .Select(x => x.ScheduledDate)
            .Should()
            .OnlyContain(d => d >= startDate && d <= endDate);

        _appointmentRepositoryMock.Verify(
            x => x.GetByDateRangeAsync(startDate, endDate),
            Times.Once
        );
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

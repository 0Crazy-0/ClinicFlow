using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorIdAndDate;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByDoctorIdAndDate;

public class GetAppointmentsByDoctorIdAndDateQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly GetAppointmentsByDoctorIdAndDateQueryHandler _sut;

    public GetAppointmentsByDoctorIdAndDateQueryHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _sut = new GetAppointmentsByDoctorIdAndDateQueryHandler(_appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenDoctorHasAppointmentsOnDate()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var date = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var query = new GetAppointmentsByDoctorIdAndDateQuery(doctorId, date, 1, 10);
        var appointments = new List<Appointment>
        {
            CreateAppointment(doctorId, date),
            CreateAppointment(doctorId, date),
        };

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByDoctorIdAndDateAsync(doctorId, date, 1, 10, It.IsAny<CancellationToken>())
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
            x => x.GetByDoctorIdAndDateAsync(doctorId, date, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenDoctorHasNoAppointmentsOnDate()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdAndDateQuery(
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime),
            1,
            10
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByDoctorIdAndDateAsync(
                    query.DoctorId,
                    query.Date,
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
                x.GetByDoctorIdAndDateAsync(
                    query.DoctorId,
                    query.Date,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private static Appointment CreateAppointment(Guid doctorId, DateOnly scheduledDate) =>
        Appointment.Schedule(
            Guid.CreateVersion7(),
            doctorId,
            Guid.CreateVersion7(),
            scheduledDate,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
}

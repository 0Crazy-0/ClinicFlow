using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly GetAppointmentsByDoctorIdQueryHandler _sut;

    public GetAppointmentsByDoctorIdQueryHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _sut = new GetAppointmentsByDoctorIdQueryHandler(_appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenDoctorHasAppointmentsOnDate()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var date = _fakeTime.GetUtcNow().UtcDateTime.Date;
        var query = new GetAppointmentsByDoctorIdQuery(doctorId, date, 1, 10);
        var appointments = new List<Appointment>
        {
            CreateAppointment(Guid.NewGuid(), doctorId, date),
            CreateAppointment(Guid.NewGuid(), doctorId, date),
        };

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByDoctorIdPaginatedAsync(doctorId, date, 1, 10, It.IsAny<CancellationToken>())
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
        result.Items.Select(x => x.DoctorId).Should().AllBeEquivalentTo(doctorId);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenDoctorHasNoAppointmentsOnDate()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.Date,
            1,
            10
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByDoctorIdPaginatedAsync(
                    query.DoctorId,
                    query.Date,
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
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
}

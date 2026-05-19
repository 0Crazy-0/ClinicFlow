using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByPatientId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByPatientId;

public class GetAppointmentsByPatientIdQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly GetAppointmentsByPatientIdQueryHandler _sut;

    public GetAppointmentsByPatientIdQueryHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _sut = new GetAppointmentsByPatientIdQueryHandler(_appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenPatientHasAppointments()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var query = new GetAppointmentsByPatientIdQuery(patientId, 1, 10);
        var appointments = new List<Appointment>
        {
            CreateAppointment(patientId, Guid.NewGuid()),
            CreateAppointment(patientId, Guid.NewGuid()),
        };

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>())
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
        result.Items.Select(x => x.PatientId).Should().AllBeEquivalentTo(patientId);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenPatientHasNoAppointments()
    {
        // Arrange
        var query = new GetAppointmentsByPatientIdQuery(Guid.NewGuid(), 1, 10);

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedAsync(
                    query.PatientId,
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

    private Appointment CreateAppointment(Guid patientId, Guid doctorId) =>
        Appointment.Schedule(
            patientId,
            doctorId,
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.Date.AddDays(1),
            TimeRange.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0))
        );
}

using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
    public async Task Handle_ShouldReturnEmptyList_WhenDoctorHasNoAppointmentsOnDate()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.Date
        );

        _appointmentRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(query.DoctorId, query.Date))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _appointmentRepositoryMock.Verify(
            x => x.GetByDoctorIdAsync(query.DoctorId, query.Date),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnAppointmentList_WhenDoctorHasAppointmentsOnDate()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var date = _fakeTime.GetUtcNow().UtcDateTime.Date;
        var query = new GetAppointmentsByDoctorIdQuery(doctorId, date);

        var appointments = new List<Appointment>
        {
            CreateAppointment(Guid.NewGuid(), doctorId, date),
            CreateAppointment(Guid.NewGuid(), doctorId, date),
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetByDoctorIdAsync(doctorId, date))
            .ReturnsAsync(appointments);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<AppointmentDto>();
        result.Select(x => x.DoctorId).Should().AllBeEquivalentTo(doctorId);

        _appointmentRepositoryMock.Verify(x => x.GetByDoctorIdAsync(doctorId, date), Times.Once);
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

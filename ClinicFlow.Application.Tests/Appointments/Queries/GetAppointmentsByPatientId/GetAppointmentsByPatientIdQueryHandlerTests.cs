using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByPatientId;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByPatientId;

public class GetAppointmentsByPatientIdQueryHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly GetAppointmentsByPatientIdQueryHandler _sut;

    public GetAppointmentsByPatientIdQueryHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _sut = new GetAppointmentsByPatientIdQueryHandler(_appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenPatientHasNoAppointments()
    {
        // Arrange
        var query = new GetAppointmentsByPatientIdQuery(Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(query.PatientId))
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _appointmentRepositoryMock.Verify(x => x.GetByPatientIdAsync(query.PatientId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnAppointmentList_WhenPatientHasAppointments()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var query = new GetAppointmentsByPatientIdQuery(patientId);

        var appointments = new List<Appointment>
        {
            CreateAppointment(patientId, Guid.NewGuid()),
            CreateAppointment(patientId, Guid.NewGuid()),
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(patientId))
            .ReturnsAsync(appointments);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<AppointmentDto>();
        result.Select(x => x.PatientId).Should().AllBeEquivalentTo(patientId);

        _appointmentRepositoryMock.Verify(x => x.GetByPatientIdAsync(patientId), Times.Once);
    }

    private static Appointment CreateAppointment(Guid patientId, Guid doctorId) =>
        Appointment.Schedule(
            patientId,
            doctorId,
            Guid.NewGuid(),
            DateTime.UtcNow.Date.AddDays(1),
            TimeRange.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0))
        );
}

using ClinicFlow.Application.Appointments.Queries.GetAppointmentById;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly GetAppointmentByIdQueryHandler _sut;

    public GetAppointmentByIdQueryHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _sut = new GetAppointmentByIdQueryHandler(_appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAppointmentDto_WhenAppointmentExists()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var query = new GetAppointmentByIdQuery(appointmentId);

        var appointment = CreateAppointment(appointmentId, patientId, doctorId);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(appointmentId);
        result.PatientId.Should().Be(patientId);
        result.DoctorId.Should().Be(doctorId);
        result.AppointmentTypeId.Should().Be(appointment.AppointmentTypeId);

        _appointmentRepositoryMock.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentDoesNotExist()
    {
        // Arrange
        var query = new GetAppointmentByIdQuery(Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(query.AppointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(query, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));
    }

    private Appointment CreateAppointment(Guid id, Guid patientId, Guid doctorId)
    {
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.Date.AddDays(1),
            TimeRange.Create(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0))
        );
        appointment.SetId(id);
        return appointment;
    }
}

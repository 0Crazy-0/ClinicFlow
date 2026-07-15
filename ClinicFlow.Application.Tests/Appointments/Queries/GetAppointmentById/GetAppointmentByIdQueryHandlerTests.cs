using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetAppointmentById;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
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
        var appointmentId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var query = new GetAppointmentByIdQuery(appointmentId);
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        appointment.SetId(appointmentId);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new AppointmentDto(
            appointment.Id,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.AppointmentTypeId,
            appointment.ScheduledDate,
            appointment.TimeRange.Start,
            appointment.TimeRange.End,
            appointment.Status,
            appointment.PatientNotes,
            appointment.ReceptionistNotes
        );

        result.Should().BeEquivalentTo(expectedDto);

        _appointmentRepositoryMock.Verify(
            x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentDoesNotExist()
    {
        // Arrange
        var query = new GetAppointmentByIdQuery(Guid.CreateVersion7());

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(query.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));

        _appointmentRepositoryMock.Verify(
            x => x.GetByIdAsync(query.AppointmentId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}

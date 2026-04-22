using ClinicFlow.Application.Appointments.EventHandlers;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.EventHandlers;

public class AppointmentCancelledEventHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly AppointmentCancelledEventHandler _sut;

    public AppointmentCancelledEventHandlerTests()
    {
        _fakeTime = new FakeTimeProvider();
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _sut = new AppointmentCancelledEventHandler(_fakeTime, _penaltyRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldApplyPenalty_WhenStatusIsLateCancellation()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var appointment = Appointment.Schedule(
            patientId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.Date,
            TimeRange.Create(
                _fakeTime.GetUtcNow().UtcDateTime.TimeOfDay,
                _fakeTime.GetUtcNow().UtcDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );

        appointment.CancelLate(
            Guid.NewGuid(),
            "Late",
            _fakeTime.GetUtcNow().UtcDateTime.AddHours(-2)
        );

        var domainEvent = new AppointmentCancelledEvent(appointment, Guid.NewGuid(), "Too late");
        var notification = new DomainEventNotification<AppointmentCancelledEvent>(domainEvent);

        _penaltyRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _penaltyRepositoryMock.Verify(
            x =>
                x.AddRangeAsync(
                    It.Is<IEnumerable<PatientPenalty>>(penalties =>
                        penalties.Any(p => p.Type == PenaltyType.Warning)
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldNotApplyPenalty_WhenStatusIsNotLateCancellation()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var appointment = Appointment.Schedule(
            patientId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.Date,
            TimeRange.Create(
                _fakeTime.GetUtcNow().UtcDateTime.TimeOfDay,
                _fakeTime.GetUtcNow().UtcDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );
        appointment.Cancel(
            Guid.NewGuid(),
            "Admin",
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-2).Date
        );

        var domainEvent = new AppointmentCancelledEvent(appointment, Guid.NewGuid(), "In time");
        var notification = new DomainEventNotification<AppointmentCancelledEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _penaltyRepositoryMock.Verify(
            x => x.GetByPatientIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _penaltyRepositoryMock.Verify(
            x =>
                x.AddRangeAsync(
                    It.IsAny<IEnumerable<PatientPenalty>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}

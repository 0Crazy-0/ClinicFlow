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
        var appointment = CreateLateCancelledAppointment(
            Guid.NewGuid(),
            patientId,
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddHours(2)
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
        var appointment = CreateCancelledAppointment(
            Guid.NewGuid(),
            patientId,
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(2)
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

    private static Appointment CreateLateCancelledAppointment(
        Guid doctorId,
        Guid patientId,
        Guid typeId,
        DateTime scheduledDateTime
    )
    {
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            scheduledDateTime.Date,
            TimeRange.Create(
                scheduledDateTime.TimeOfDay,
                scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );

        appointment.Cancel(
            Guid.NewGuid(),
            "Late",
            MedicalSpecialty.Create("Cardiology", "Heart", 60, 24),
            scheduledDateTime.AddHours(-2),
            false
        );

        return appointment;
    }

    private static Appointment CreateCancelledAppointment(
        Guid doctorId,
        Guid patientId,
        Guid typeId,
        DateTime scheduledDateTime
    )
    {
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            scheduledDateTime.Date,
            TimeRange.Create(
                scheduledDateTime.TimeOfDay,
                scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );

        appointment.Cancel(
            Guid.NewGuid(),
            "Admin",
            MedicalSpecialty.Create("Cardiology", "Heart", 60, 24),
            scheduledDateTime.AddDays(-2),
            true
        );

        return appointment;
    }
}

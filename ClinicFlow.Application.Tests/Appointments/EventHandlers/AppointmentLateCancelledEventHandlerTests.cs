using AwesomeAssertions;
using ClinicFlow.Application.Appointments.EventHandlers;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.EventHandlers;

public class AppointmentLateCancelledEventHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly AppointmentLateCancelledEventHandler _sut;

    public AppointmentLateCancelledEventHandlerTests()
    {
        _fakeTime = new FakeTimeProvider();
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new AppointmentLateCancelledEventHandler(
            _fakeTime,
            _penaltyRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreatePenaltiesWithCorrectProperties()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var appointment = Appointment.Schedule(
            patientId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        appointment.CancelLate(
            Guid.NewGuid(),
            "Late",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddHours(-2))
        );

        var domainEvent = new AppointmentLateCancelledEvent(
            appointment,
            Guid.NewGuid(),
            "Too late"
        );

        var notification = new DomainEventNotification<AppointmentLateCancelledEvent>(domainEvent);

        IEnumerable<PatientPenalty>? capturedPenalties = null;
        _penaltyRepositoryMock
            .Setup(x => x.GetHistoryByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _penaltyRepositoryMock
            .Setup(x =>
                x.CreateRangeAsync(
                    It.IsAny<IEnumerable<PatientPenalty>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<IEnumerable<PatientPenalty>, CancellationToken>(
                (penalties, _) => capturedPenalties = penalties
            );

        // Act
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

        // Assert
        var penalty = capturedPenalties.Should().ContainSingle().Subject;

        penalty.Type.Should().Be(PenaltyType.Warning);
        penalty.Reason.Should().Be(PenaltyReasons.LateCancellation);
        penalty.PatientId.Should().Be(patientId);
        capturedPenalties.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateRangeAndSaveChanges()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var appointment = Appointment.Schedule(
            patientId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        appointment.CancelLate(
            Guid.NewGuid(),
            "Late",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddHours(-2))
        );

        var domainEvent = new AppointmentLateCancelledEvent(
            appointment,
            Guid.NewGuid(),
            "Too late"
        );

        var notification = new DomainEventNotification<AppointmentLateCancelledEvent>(domainEvent);

        _penaltyRepositoryMock
            .Setup(x => x.GetHistoryByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

        // Assert
        _penaltyRepositoryMock.Verify(
            x =>
                x.CreateRangeAsync(
                    It.IsAny<IEnumerable<PatientPenalty>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

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

public class AppointmentMarkedAsNoShowEventHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _patientPenaltyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly AppointmentMarkedAsNoShowEventHandler _sut;

    public AppointmentMarkedAsNoShowEventHandlerTests()
    {
        _patientPenaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fakeTime = new FakeTimeProvider();
        _sut = new AppointmentMarkedAsNoShowEventHandler(
            _fakeTime,
            _patientPenaltyRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreatePenaltiesWithCorrectProperties()
    {
        // Arrange
        var appointment = CreateAppointment();
        var domainEvent = new AppointmentMarkedAsNoShowEvent(appointment);
        var notification = new DomainEventNotification<AppointmentMarkedAsNoShowEvent>(domainEvent);

        IEnumerable<PatientPenalty>? capturedPenalties = null;
        _patientPenaltyRepositoryMock
            .Setup(x =>
                x.GetHistoryByPatientIdAsync(appointment.PatientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        _patientPenaltyRepositoryMock
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
        penalty.Reason.Should().Be(PenaltyReasons.NoShow);
        penalty.PatientId.Should().Be(appointment.PatientId);
        capturedPenalties.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateRangeAndSaveChanges()
    {
        // Arrange
        var appointment = CreateAppointment();
        var domainEvent = new AppointmentMarkedAsNoShowEvent(appointment);
        var notification = new DomainEventNotification<AppointmentMarkedAsNoShowEvent>(domainEvent);

        _patientPenaltyRepositoryMock
            .Setup(x =>
                x.GetHistoryByPatientIdAsync(appointment.PatientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        // Act
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

        // Assert
        _patientPenaltyRepositoryMock.Verify(
            x =>
                x.CreateRangeAsync(
                    It.IsAny<IEnumerable<PatientPenalty>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private Appointment CreateAppointment() =>
        Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
}

using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.CleanExpiredDisplacedAppointments;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.CleanExpiredDisplacedAppointments;

public class CleanExpiredDisplacedAppointmentsCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly CleanExpiredDisplacedAppointmentsCommandHandler _sut;

    public CleanExpiredDisplacedAppointmentsCommandHandlerTests()
    {
        _sut = new CleanExpiredDisplacedAppointmentsCommandHandler(
            _fakeTime,
            _appointmentRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCancelAllExpiredDisplacedAppointments()
    {
        // Arrange
        var appointment1 = CreateDisplacedAppointment(
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1)
        );
        var appointment2 = CreateDisplacedAppointment(
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(-3)
        );

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetExpiredDisplacedAppointmentsAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([appointment1, appointment2]);

        // Act
        await _sut.Handle(
            new CleanExpiredDisplacedAppointmentsCommand(),
            TestContext.Current.CancellationToken
        );

        // Assert
        appointment1.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment2.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment1.CancelledByUserId.Should().BeNull();
        appointment2.CancelledByUserId.Should().BeNull();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldStillCallSaveChanges_WhenNoExpiredAppointmentsExist()
    {
        // Arrange
        _appointmentRepositoryMock
            .Setup(x =>
                x.GetExpiredDisplacedAppointmentsAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        // Act
        await _sut.Handle(
            new CleanExpiredDisplacedAppointmentsCommand(),
            TestContext.Current.CancellationToken
        );

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Appointment CreateDisplacedAppointment(DateTime scheduledDate)
    {
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(scheduledDate),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        appointment.MarkAsRequiresReassignment();
        return appointment;
    }
}

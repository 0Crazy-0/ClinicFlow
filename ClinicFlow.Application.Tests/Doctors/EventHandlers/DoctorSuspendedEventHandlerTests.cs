using AwesomeAssertions;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Doctors.EventHandlers;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Doctors;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Doctors.EventHandlers;

public class DoctorSuspendedEventHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly DoctorSuspendedEventHandler _sut;

    public DoctorSuspendedEventHandlerTests()
    {
        _fakeTime = new FakeTimeProvider();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new DoctorSuspendedEventHandler(
            _fakeTime,
            _appointmentRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldTransitionAllFutureAppointmentsToRequiresReassignment()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointment1 = CreateFutureScheduledAppointment(doctorId);
        var appointment2 = CreateFutureScheduledAppointment(doctorId);

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([appointment1, appointment2]);

        var domainEvent = new DoctorSuspendedEvent(doctorId);
        var notification = new DomainEventNotification<DoctorSuspendedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

        // Assert
        appointment1.Status.Should().Be(AppointmentStatus.RequiresReassignment);
        appointment2.Status.Should().Be(AppointmentStatus.RequiresReassignment);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenDoctorHasNoFutureAppointments()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        var domainEvent = new DoctorSuspendedEvent(doctorId);
        var notification = new DomainEventNotification<DoctorSuspendedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private Appointment CreateFutureScheduledAppointment(Guid doctorId) =>
        Appointment.Schedule(
            Guid.CreateVersion7(),
            doctorId,
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
}

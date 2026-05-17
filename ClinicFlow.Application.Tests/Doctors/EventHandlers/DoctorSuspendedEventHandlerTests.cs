using ClinicFlow.Application.Common.Models;
using ClinicFlow.Application.Doctors.EventHandlers;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Doctors;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
        var doctorId = Guid.NewGuid();
        var referenceDate = _fakeTime.GetUtcNow().UtcDateTime.Date;
        var appointment1 = CreateScheduledAppointment(doctorId, referenceDate.AddDays(1));
        var appointment2 = CreateScheduledAppointment(doctorId, referenceDate.AddDays(5));

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([appointment1, appointment2]);

        var domainEvent = new DoctorSuspendedEvent(doctorId);
        var notification = new DomainEventNotification<DoctorSuspendedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        appointment1.Status.Should().Be(AppointmentStatus.RequiresReassignment);
        appointment2.Status.Should().Be(AppointmentStatus.RequiresReassignment);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenDoctorHasNoFutureAppointments()
    {
        // Arrange
        var doctorId = Guid.NewGuid();

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetFutureScheduledByDoctorIdAsync(
                    doctorId,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        var domainEvent = new DoctorSuspendedEvent(doctorId);
        var notification = new DomainEventNotification<DoctorSuspendedEvent>(domainEvent);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Appointment CreateScheduledAppointment(Guid doctorId, DateTime scheduledDate) =>
        Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            scheduledDate.Date,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10))
        );
}

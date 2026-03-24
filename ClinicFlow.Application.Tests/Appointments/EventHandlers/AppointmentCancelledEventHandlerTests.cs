using System.Reflection;
using ClinicFlow.Application.Appointments.EventHandlers;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.EventHandlers;

public class AppointmentCancelledEventHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly AppointmentCancelledEventHandler _sut;

    public AppointmentCancelledEventHandlerTests()
    {
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _sut = new AppointmentCancelledEventHandler(_penaltyRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldApplyPenalty_WhenStatusIsLateCancellation()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointment = CreateAppointment(
            appointmentId,
            Guid.NewGuid(),
            patientId,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(2),
            true
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
        var appointmentId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointment = CreateAppointment(
            appointmentId,
            Guid.NewGuid(),
            patientId,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(2),
            false
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

    private static Appointment CreateAppointment(
        Guid id,
        Guid doctorId,
        Guid patientId,
        Guid typeId,
        DateTime scheduledDateTime,
        bool isLateCancellation
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
        SetPrivateProperty(appointment, nameof(Appointment.Id), id);

        if (isLateCancellation)
        {
            SetPrivateProperty(
                appointment,
                nameof(Appointment.Status),
                AppointmentStatus.LateCancellation
            );
        }
        else
        {
            SetPrivateProperty(
                appointment,
                nameof(Appointment.Status),
                AppointmentStatus.Cancelled
            );
        }

        return appointment;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var prop = type.GetProperty(
                propertyName,
                BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly
            );
            if (prop != null)
            {
                prop.SetValue(obj, value);
                return;
            }
            type = type.BaseType;
        }
    }
}

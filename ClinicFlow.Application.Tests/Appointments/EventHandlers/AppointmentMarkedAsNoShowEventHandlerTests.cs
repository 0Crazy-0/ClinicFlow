using System.Reflection;
using ClinicFlow.Application.Appointments.EventHandlers;
using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.EventHandlers;

public class AppointmentMarkedAsNoShowEventHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _patientPenaltyRepositoryMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly AppointmentMarkedAsNoShowEventHandler _sut;

    public AppointmentMarkedAsNoShowEventHandlerTests()
    {
        _patientPenaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _fakeTime = new FakeTimeProvider();
        _sut = new AppointmentMarkedAsNoShowEventHandler(
            _fakeTime,
            _patientPenaltyRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldApplyNoShowPenalty()
    {
        // Arrange
        var appointment = CreateAppointment(Guid.NewGuid(), _fakeTime.GetUtcNow().UtcDateTime);

        var domainEvent = new AppointmentMarkedAsNoShowEvent(appointment);
        var notification = new DomainEventNotification<AppointmentMarkedAsNoShowEvent>(domainEvent);

        _patientPenaltyRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _patientPenaltyRepositoryMock.Verify(
            x =>
                x.AddRangeAsync(
                    It.Is<IEnumerable<PatientPenalty>>(penalties =>
                        penalties.Any(p =>
                            p.Type == PenaltyType.Warning && p.Reason == PenaltyReasons.NoShow
                        )
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private static Appointment CreateAppointment(Guid id, DateTime referenceTime)
    {
        var scheduledDateTime = referenceTime.AddDays(1);
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDateTime.Date,
            TimeRange.Create(
                scheduledDateTime.TimeOfDay,
                scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );
        SetPrivateProperty(appointment, nameof(Appointment.Id), id);
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

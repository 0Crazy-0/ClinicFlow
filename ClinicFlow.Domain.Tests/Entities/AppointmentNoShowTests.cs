using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentNoShowTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void MarkAsNoShowByStaff_ShouldSucceed()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        appointment.MarkAsNoShowByStaff();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        appointment.DomainEvents.OfType<AppointmentMarkedAsNoShowEvent>().Should().ContainSingle();
    }

    [Fact]
    public void MarkAsNoShowByDoctor_ShouldSucceed_WhenDoctorIdMatches()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        appointment.MarkAsNoShowByDoctor(appointment.DoctorId);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        appointment.DomainEvents.OfType<AppointmentMarkedAsNoShowEvent>().Should().ContainSingle();
    }

    [Fact]
    public void MarkAsNoShowByDoctor_ShouldThrowUnauthorized_WhenDoctorIdDoesNotMatch()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        var act = () => appointment.MarkAsNoShowByDoctor(Guid.CreateVersion7());

        // Assert
        act.Should()
            .Throw<AppointmentNoShowUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedNoShow);
    }

    [Fact]
    public void MarkAsNoShowByStaff_ShouldSetStatusToNoShow_WhenStatusIsScheduled()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.ClearDomainEvents();

        // Act
        appointment.MarkAsNoShowByStaff();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        appointment.DomainEvents.OfType<AppointmentMarkedAsNoShowEvent>().Should().ContainSingle();
    }

    [Fact]
    public void MarkAsNoShowByStaff_ShouldThrowException_WhenStatusIsCancelled()
    {
        // Arrange
        var appointment = CreateAppointment();

        appointment.Cancel(
            Guid.CreateVersion7(),
            "Reason",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
        );

        // Act && Assert
        appointment
            .Invoking(x => x.MarkAsNoShowByStaff())
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotMarkNoShow);
    }

    private Appointment CreateAppointment() =>
        Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(
                TimeOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
                TimeOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).AddHours(1))
            )
        );
}

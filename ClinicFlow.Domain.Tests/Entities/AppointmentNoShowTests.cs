using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentNoShowTests
{
    [Fact]
    public void MarkAsNoShowByStaff_ShouldSucceed()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

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
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

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
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        // Act
        var act = () => appointment.MarkAsNoShowByDoctor(Guid.NewGuid());

        // Assert
        act.Should()
            .Throw<AppointmentNoShowUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedNoShow);
    }

    [Fact]
    public void MarkAsNoShowByStaff_ShouldSetStatusToNoShow_WhenStatusIsConfirmed()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(1));
        appointment.Confirm();
        appointment.ClearDomainEvents();

        // Act
        appointment.MarkAsNoShowByStaff();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentMarkedAsNoShowEvent);
    }

    [Fact]
    public void MarkAsNoShowByStaff_ShouldThrowException_WhenStatusIsCancelled()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));
        var specialty = CreateSpecialty(24);
        appointment.Cancel(Guid.NewGuid(), "Reason", specialty);

        // Act && Assert
        appointment
            .Invoking(x => x.MarkAsNoShowByStaff())
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.CannotMarkNoShow);
    }

    private static Appointment CreateAppointment(DateTime scheduledDateTime) =>
        Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDateTime.Date,
            TimeRange.Create(
                scheduledDateTime.TimeOfDay,
                scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );

    private static MedicalSpecialty CreateSpecialty(int minCancellationHours) =>
        MedicalSpecialty.Create("Test Specialty", "Description", 30, minCancellationHours);
}

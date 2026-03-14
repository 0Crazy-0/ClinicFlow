using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentNoShowServiceTests
{
    [Theory]
    [InlineData(UserRole.Admin, false)]
    [InlineData(UserRole.Receptionist, false)]
    [InlineData(UserRole.Doctor, true)]
    public void MarkAsNoShow_ShouldSucceed_WhenAuthorized(UserRole role, bool isOwn)
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        Guid? doctorId = null;

        if (role is UserRole.Doctor) doctorId = isOwn ? appointment.DoctorId : Guid.NewGuid();

        // Act
        var context = new AppointmentNoShowContext { InitiatorRole = role, InitiatorDoctorId = doctorId };
        var result = AppointmentNoShowService.MarkAsNoShow(appointment, context, []).ToList();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        appointment.DomainEvents.OfType<AppointmentMarkedAsNoShowEvent>().Should().ContainSingle();

        result.Should().NotBeNull();
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning && p.Reason == "No show");
    }

    [Theory]
    [InlineData(UserRole.Doctor, DomainErrors.Appointment.CannotMarkNoShow)]
    [InlineData(UserRole.Patient, DomainErrors.Appointment.CannotMarkNoShow)]
    public void MarkAsNoShow_ShouldThrowUnauthorized_WhenNotAuthorized(UserRole role, string expectedMessage)
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));
        Guid? doctorId = role is UserRole.Doctor ? Guid.NewGuid() : null;

        // Act
        var context = new AppointmentNoShowContext { InitiatorRole = role, InitiatorDoctorId = doctorId };
        var act = () => AppointmentNoShowService.MarkAsNoShow(appointment, context, []);

        // Assert
        act.Should().Throw<AppointmentNoShowUnauthorizedException>().WithMessage(expectedMessage);
    }


    [Fact]
    public void MarkAsNoShow_ShouldThrowUnauthorized_WhenDoctorIdDoesNotMatchAppointmentDoctorId()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));
        var doctorId = Guid.NewGuid();

        // Act
        var context = new AppointmentNoShowContext { InitiatorRole = UserRole.Doctor, InitiatorDoctorId = doctorId };
        var act = () => AppointmentNoShowService.MarkAsNoShow(appointment, context, []);

            // Assert
            act.Should().Throw<AppointmentNoShowUnauthorizedException>().WithMessage(DomainErrors.Appointment.CannotMarkNoShow);
    }

    [Fact]
    public void MarkAsNoShow_ShouldThrowUnauthorized_WhenDoctorIdIsNull()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        // Act
        var context = new AppointmentNoShowContext { InitiatorRole = UserRole.Doctor, InitiatorDoctorId = null };
        var act = () => AppointmentNoShowService.MarkAsNoShow(appointment, context, []);

        // Assert
        act.Should().Throw<AppointmentNoShowUnauthorizedException>().WithMessage(DomainErrors.Appointment.CannotMarkNoShow);
    }

    // Helpers

    private static Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        scheduledDateTime.Date, TimeRange.Create(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));
}

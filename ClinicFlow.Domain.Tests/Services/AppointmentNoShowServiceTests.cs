using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

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


        var user = CreateUser(role, doctorId);
        var existingPenalties = Enumerable.Empty<PatientPenalty>();

        // Act
        var result = AppointmentNoShowService.MarkAsNoShow(appointment, user, existingPenalties).ToList();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        appointment.DomainEvents.OfType<AppointmentMarkedAsNoShowEvent>().Should().ContainSingle();

        result.Should().NotBeNull();
        result.Should().ContainSingle(p => p.Type == PenaltyType.Warning && p.Reason == "No show");
    }

    [Theory]
    [InlineData(UserRole.Doctor, false, "Doctors can only mark their own appointments as No-Show.")]
    [InlineData(UserRole.Patient, true, "User is not authorized to mark this appointment as No-Show.")]
    [InlineData(UserRole.Patient, false, "User is not authorized to mark this appointment as No-Show.")]
    public void MarkAsNoShow_ShouldThrowUnauthorized_WhenNotAuthorized(UserRole role, bool isOwn, string expectedMessage)
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        Guid? doctorId = role is UserRole.Doctor && !isOwn ? Guid.NewGuid() : null;
        Guid? patientId = role is UserRole.Patient && isOwn ? appointment.PatientId : null;

        var user = CreateUser(role, doctorId, patientId);
        var existingPenalties = Enumerable.Empty<PatientPenalty>();

        // Act
        var act = () => AppointmentNoShowService.MarkAsNoShow(appointment, user, existingPenalties);

        // Assert
        act.Should().Throw<AppointmentCancellationUnauthorizedException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void MarkAsNoShow_ShouldThrowUnauthorized_WhenDoctorIdDoesNotMatchAppointmentDoctorId()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        // A doctor user with a completely different DoctorId (and also test if DoctorId is somehow null)
        var user = CreateUser(UserRole.Doctor, doctorId: Guid.NewGuid());
        var existingPenalties = Enumerable.Empty<PatientPenalty>();

        // Act
        var act = () => AppointmentNoShowService.MarkAsNoShow(appointment, user, existingPenalties);

        // Assert
        act.Should().Throw<AppointmentCancellationUnauthorizedException>().WithMessage("Doctors can only mark their own appointments as No-Show.");
    }

    [Fact]
    public void MarkAsNoShow_ShouldThrowUnauthorized_WhenDoctorIdIsNull()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        // A doctor user where DoctorId is unexpectedly null
        var user = CreateUser(UserRole.Doctor, doctorId: null);
        var existingPenalties = Enumerable.Empty<PatientPenalty>();

        // Act
        var act = () => AppointmentNoShowService.MarkAsNoShow(appointment, user, existingPenalties);

        // Assert
        act.Should().Throw<AppointmentCancellationUnauthorizedException>().WithMessage("Doctors can only mark their own appointments as No-Show.");
    }

    // Helpers

    private static Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        scheduledDateTime.Date, TimeRange.Create(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));

    private static User CreateUser(UserRole role, Guid? doctorId = null, Guid? patientId = null) => User.Create(EmailAddress.Create("test@clinic.com"),
        "hashedpassword", PersonName.Create("Test User"), PhoneNumber.Create("555-0000"), role, doctorId, patientId);
}

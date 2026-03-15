using ClinicFlow.Domain.Common;
using System.Reflection;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentCancellationServiceTests
{
    [Theory]
    [InlineData(UserRole.Admin, false, false, AppointmentCategory.Checkup)]
    [InlineData(UserRole.Doctor, true, false, AppointmentCategory.Checkup)]
    [InlineData(UserRole.Patient, true, false, AppointmentCategory.Checkup)]
    [InlineData(UserRole.Patient, false, true, AppointmentCategory.Checkup)]
    public void CancelAppointment_ShouldSucceed_WhenAuthorized(UserRole role, bool isOwn, bool isFamily, AppointmentCategory typeEnum)
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        Guid? doctorId = null;
        Guid? patientId = null;

        if (role is UserRole.Doctor) doctorId = isOwn ? appointment.DoctorId : Guid.NewGuid();
        else if (role is UserRole.Patient) patientId = isOwn ? appointment.PatientId : Guid.NewGuid();

        var type = CreateAppointmentType(typeEnum);
        var user = CreateUser(role);

        // Act
        var context = new AppointmentCancellationContext
        {
            Initiator = user,
            InitiatorDoctorId = doctorId,
            InitiatorPatientId = patientId,
            AppointmentTypeDefinition = type,
            IsAuthorizedFamilyMember = isFamily,
            Specialty = CreateSpecialty(24),
            Reason = "Valid Reason"
        };
        AppointmentCancellationService.CancelAppointment(appointment, context);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);

        if (role is UserRole.Admin) appointment.CancelledByUserId.Should().Be(user.Id);

        appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Should().ContainSingle();
    }

    [Theory]
    [InlineData(UserRole.Doctor, false, false, AppointmentCategory.Checkup)]
    [InlineData(UserRole.Patient, false, true, AppointmentCategory.Procedure)]
    [InlineData(UserRole.Patient, false, false, AppointmentCategory.Checkup)]
    public void CancelAppointment_ShouldThrowUnauthorized_WhenNotAuthorized(UserRole role, bool isOwn, bool isFamily, AppointmentCategory typeEnum)
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        // Ensure we don't accidentally match IDs
        Guid? doctorId = role is UserRole.Doctor && !isOwn ? Guid.NewGuid() : null;
        Guid? patientId = role is UserRole.Patient && !isOwn ? Guid.NewGuid() : null;

        var type = CreateAppointmentType(typeEnum);
        var user = CreateUser(role);

        // Act
        var context = new AppointmentCancellationContext
        {
            Initiator = user,
            InitiatorDoctorId = doctorId,
            InitiatorPatientId = patientId,
            AppointmentTypeDefinition = type,
            IsAuthorizedFamilyMember = isFamily,
            Specialty = CreateSpecialty(24),
            Reason = "Reason"
        };

        var act = () => AppointmentCancellationService.CancelAppointment(appointment, context);

        // Assert
        act.Should().Throw<AppointmentCancellationUnauthorizedException>().WithMessage(DomainErrors.Appointment.UnauthorizedCancellation);
    }

    [Fact]
    public void CancelAppointment_ShouldThrowBusinessRuleValidationException_WhenStaffCancelsWithoutReason()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));
        var receptionist = CreateUser(UserRole.Receptionist);
        var type = CreateAppointmentType(AppointmentCategory.Checkup);

        // Act
        var context = new AppointmentCancellationContext
        {
            Initiator = receptionist,
            AppointmentTypeDefinition = type,
            IsAuthorizedFamilyMember = false,
            Specialty = CreateSpecialty(24),
            Reason = ""
        };

        var act = () => AppointmentCancellationService.CancelAppointment(appointment, context);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage(DomainErrors.Appointment.MissingCancellationReason);
    }

    // Helpers

    private static MedicalSpecialty CreateSpecialty(int minCancellationHours)
    {
        var specialty = (MedicalSpecialty)Activator.CreateInstance(typeof(MedicalSpecialty), true)!;

        SetPrivateProperty(specialty, nameof(MedicalSpecialty.MinCancellationHours), minCancellationHours);

        return specialty;
    }

    private static Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDateTime.Date,
        TimeRange.Create(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));

    private static User CreateUser(UserRole role) => User.Create(EmailAddress.Create("test@clinic.com"), "hashedpassword",
        PhoneNumber.Create("555-0000"), role);

    private static AppointmentTypeDefinition CreateAppointmentType(AppointmentCategory typeEnum) => AppointmentTypeDefinition.Create(typeEnum, typeEnum.ToString(),
        "Test description", TimeSpan.FromMinutes(30));

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();

        while (type != null)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (prop != null)
            {
                prop.SetValue(obj, value);
                return;
            }

            type = type.BaseType;
        }
    }
}

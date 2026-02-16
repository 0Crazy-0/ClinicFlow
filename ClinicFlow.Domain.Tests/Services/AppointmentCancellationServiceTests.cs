using System.Reflection;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentCancellationServiceTests
{
    private readonly AppointmentCancellationService _sut;
    private readonly Mock<IMedicalSpecialtyRepository> _medicalSpecialtyRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;

    public AppointmentCancellationServiceTests()
    {
        _medicalSpecialtyRepositoryMock = new Mock<IMedicalSpecialtyRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _sut = new AppointmentCancellationService(_medicalSpecialtyRepositoryMock.Object, _doctorRepositoryMock.Object);
    }

    [Theory]
    [InlineData(UserRoleEnum.Admin, false, false, AppointmentTypeEnum.Checkup)]
    [InlineData(UserRoleEnum.Doctor, true, false, AppointmentTypeEnum.Checkup)]
    [InlineData(UserRoleEnum.Patient, true, false, AppointmentTypeEnum.Checkup)]
    [InlineData(UserRoleEnum.Patient, false, true, AppointmentTypeEnum.Checkup)]
    public async Task CancelAppointment_ShouldSucceed_WhenAuthorized(UserRoleEnum role, bool isOwn, bool isFamily, AppointmentTypeEnum typeEnum)
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        Guid? doctorId = null;
        Guid? patientId = null;

        if (role is UserRoleEnum.Doctor) doctorId = isOwn ? appointment.DoctorId : Guid.NewGuid();
        else if (role is UserRoleEnum.Patient) patientId = isOwn ? appointment.PatientId : Guid.NewGuid();

        var type = CreateAppointmentType(typeEnum);
        var user = CreateUser(role, doctorId, patientId);

        SetupRepositories(appointment.DoctorId, 24); // 24h min cancellation

        // Act
        await _sut.CancelAppointmentAsync(appointment, user, type, isFamily, "Valid Reason");

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.Cancelled);

        if (role is UserRoleEnum.Admin) appointment.CancelledByUserId.Should().Be(user.Id);

        appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Single();
    }

    [Theory]
    [InlineData(UserRoleEnum.Doctor, false, false, AppointmentTypeEnum.Checkup, "Doctors can only cancel their own appointments.")]
    [InlineData(UserRoleEnum.Patient, false, true, AppointmentTypeEnum.Procedure, "Family members cannot cancel appointments of type: Procedure")]
    [InlineData(UserRoleEnum.Patient, false, false, AppointmentTypeEnum.Checkup, "User is not authorized to cancel this appointment.")]
    public async Task CancelAppointment_ShouldThrowUnauthorized_WhenNotAuthorized(UserRoleEnum role, bool isOwn, bool isFamily, AppointmentTypeEnum typeEnum, string expectedMessage)
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));

        // Ensure we don't accidentally match IDs
        Guid? doctorId = role is UserRoleEnum.Doctor && !isOwn ? Guid.NewGuid() : null;
        Guid? patientId = role is UserRoleEnum.Patient && !isOwn ? Guid.NewGuid() : null;

        var type = CreateAppointmentType(typeEnum);
        var user = CreateUser(role, doctorId, patientId);

        // Act
        var act = () => _sut.CancelAppointmentAsync(appointment, user, type, isFamily, "Reason");

        // Assert
        await act.Should().ThrowAsync<AppointmentCancellationUnauthorizedException>().WithMessage(expectedMessage);
    }

    [Fact]
    public async Task CancelAppointment_ShouldThrowBusinessRuleValidationException_WhenStaffCancelsWithoutReason()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddDays(2));
        var receptionist = CreateUser(UserRoleEnum.Receptionist);
        var type = CreateAppointmentType(AppointmentTypeEnum.Checkup);

        // Act
        var act = () => _sut.CancelAppointmentAsync(appointment, receptionist, type, false, "");

        // Assert
        await act.Should().ThrowAsync<BusinessRuleValidationException>().WithMessage("Staff members must provide a reason for cancellation.");
    }

    // Helpers

    private void SetupRepositories(Guid doctorId, int minCancellationHours)
    {
        var doctor = (Doctor)Activator.CreateInstance(typeof(Doctor), true)!;

        SetPrivateProperty(doctor, nameof(Doctor.MedicalSpecialtyId), Guid.NewGuid());
        
        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);

        var specialty = (MedicalSpecialty)Activator.CreateInstance(typeof(MedicalSpecialty), true)!;
        
        SetPrivateProperty(specialty, nameof(MedicalSpecialty.MinCancellationHours), minCancellationHours);

        _medicalSpecialtyRepositoryMock.Setup(x => x.GetByIdAsync(doctor.MedicalSpecialtyId)).ReturnsAsync(specialty);
    }

    private static Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDateTime.Date,
        new TimeRange(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));

    private static User CreateUser(UserRoleEnum role, Guid? doctorId = null, Guid? patientId = null)
    {
        return User.Create("test@clinic.com", "hashedpassword", "Test User", "555-0000", role, doctorId, patientId);
    }

    private static AppointmentType CreateAppointmentType(AppointmentTypeEnum typeEnum)
    {
        return AppointmentType.Create(typeEnum, typeEnum.ToString(), "Test description", TimeSpan.FromMinutes(30));
    }

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

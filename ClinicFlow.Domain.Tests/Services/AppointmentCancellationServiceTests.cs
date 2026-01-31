using System.Reflection;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentCancellationServiceTests
{
    private readonly AppointmentCancellationService _sut;

    public AppointmentCancellationServiceTests()
    {
        _sut = new AppointmentCancellationService();
    }

    #region CancelAppointment Tests

    [Fact]
    public void CancelAppointment_ShouldCancel_WhenAdminCancels()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(48));
        var adminUser = CreateUser(UserRoleEnum.Admin);
        var type = CreateAppointmentType(AppointmentTypeEnum.Checkup);

        // Act
        _sut.CancelAppointment(appointment, adminUser, type, false, "Admin Override", 24);

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.Cancelled);
        appointment.CancelledByUserId.Should().Be(adminUser.Id);
        appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Single();

    }

    [Fact]
    public void CancelAppointment_ShouldThrowBusinessRuleValidationException_WhenStaffCancelsWithoutReason()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(48));
        var receptionist = CreateUser(UserRoleEnum.Receptionist);
        var type = CreateAppointmentType(AppointmentTypeEnum.Checkup);

        // Act
        var act = () => _sut.CancelAppointment(appointment, receptionist, type, false, "", 24);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Staff members must provide a reason for cancellation.");
    }

    [Fact]
    public void CancelAppointment_ShouldSucceed_WhenDoctorCancelsOwnAppointment()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(48));
        var doctorUser = CreateUser(UserRoleEnum.Doctor, doctorId: appointment.DoctorId);
        var type = CreateAppointmentType(AppointmentTypeEnum.Checkup);

        // Act
        _sut.CancelAppointment(appointment, doctorUser, type, false, "Not feeling well", 24);

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.Cancelled);
    }

    [Fact]
    public void CancelAppointment_ShouldThrowUnauthorized_WhenDoctorCancelsOtherAppointment()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(48));
        var otherDoctor = CreateUser(UserRoleEnum.Doctor, doctorId: Guid.NewGuid());
        var type = CreateAppointmentType(AppointmentTypeEnum.Checkup);

        // Act
        var act = () => _sut.CancelAppointment(appointment, otherDoctor, type, false, "Mistake", 24);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>().WithMessage("Doctors can only cancel their own appointments.");
    }

    [Fact]
    public void CancelAppointment_ShouldSucceed_WhenPatientCancelsOwnAppointment()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(48));
        var patientUser = CreateUser(UserRoleEnum.Patient, patientId: appointment.PatientId);
        var type = CreateAppointmentType(AppointmentTypeEnum.Checkup);

        // Act
        _sut.CancelAppointment(appointment, patientUser, type, false, null, 24); // Optional reason

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.Cancelled);
    }

    [Fact]
    public void CancelAppointment_ShouldSucceed_WhenFamilyMemberCancelsAllowedType()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(48));
        var familyUser = CreateUser(UserRoleEnum.Patient, patientId: Guid.NewGuid()); // Different ID
        var type = CreateAppointmentType(AppointmentTypeEnum.Checkup); // Allowed type

        // Act
        _sut.CancelAppointment(appointment, familyUser, type, true, "Mom can't make it", 24);

        // Assert
        appointment.Status.Should().Be(AppointmentStatusEnum.Cancelled);
    }

    [Fact]
    public void CancelAppointment_ShouldThrowUnauthorized_WhenFamilyMemberCancelsRestrictedType()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(48));
        var familyUser = CreateUser(UserRoleEnum.Patient, patientId: Guid.NewGuid());
        var type = CreateAppointmentType(AppointmentTypeEnum.Procedure); // Restricted

        // Act
        var act = () => _sut.CancelAppointment(appointment, familyUser, type, true, "Grandma fails", 24);

        // Assert
        act.Should().Throw<UnauthorizedAccessException>().WithMessage($"Family members cannot cancel appointments of type: {AppointmentTypeEnum.Procedure}");
    }

    [Fact]
    public void CancelAppointment_ShouldThrowUnauthorized_WhenUnrelatedPatientCancels()
    {
        // Arrange
        var appointment = CreateAppointment(DateTime.UtcNow.AddHours(48));
        var stranger = CreateUser(UserRoleEnum.Patient, patientId: Guid.NewGuid());
        var type = CreateAppointmentType(AppointmentTypeEnum.Checkup);

        // Act
        var act = () => _sut.CancelAppointment(appointment, stranger, type, false, "Hacking", 24); // isAuthorizedFamilyMember = false

        // Assert
        act.Should().Throw<UnauthorizedAccessException>().WithMessage("User is not authorized to cancel this appointment.");
    }

    #endregion

    #region Helpers

    private Appointment CreateAppointment(DateTime scheduledDateTime) => Appointment.Schedule(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDateTime.Date,
        new TimeRange(scheduledDateTime.TimeOfDay, scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));

    private User CreateUser(UserRoleEnum role, Guid? doctorId = null, Guid? patientId = null)
    {
        var user = new User();

        SetPrivateProperty(user, "Role", role); // Assuming user has 'Id' which is auto generated or we rely on default
        SetPrivateProperty(user, "Id", Guid.NewGuid());

        if (doctorId.HasValue) SetPrivateProperty(user, "DoctorId", doctorId.Value);
        if (patientId.HasValue) SetPrivateProperty(user, "PatientId", patientId.Value);
        
        return user;
    }

    private AppointmentType CreateAppointmentType(AppointmentTypeEnum typeEnum)
    {
        var type = (AppointmentType)Activator.CreateInstance(typeof(AppointmentType), true)!;

        SetPrivateProperty(type, "Type", typeEnum);
        
        return type;
    }

    private void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();

        PropertyInfo? prop = null;
        
        while (type != null)
        {
            prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (prop != null && prop.GetSetMethod(true) != null) break;

            type = type.BaseType;
        }

        if (prop != null)
        {
            prop.SetValue(obj, value);
        }
        else
        {
            // Fallback for when property is not found or has no setter - try backing field
            type = obj.GetType();

            while (type != null)
            {
                var field = type.GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
                
                type = type.BaseType;
            }
        }
    }
    #endregion
}

using System.Reflection;
using ClinicFlow.Application.Appointments.Commands.CancelAppointment;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IMedicalSpecialtyRepository> _specialtyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly CancelAppointmentCommandHandler _sut;

    public CancelAppointmentCommandHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _specialtyRepositoryMock = new Mock<IMedicalSpecialtyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new CancelAppointmentCommandHandler(_appointmentRepositoryMock.Object, _userRepositoryMock.Object, _appointmentTypeRepositoryMock.Object,
             _doctorRepositoryMock.Object, _patientRepositoryMock.Object, _specialtyRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCancelAppointment_WhenAllEntitiesExistAndValid()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), false, "Patient request");

        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();

        var appointment = CreateAppointment(command.AppointmentId, doctorId, patientId, appointmentTypeId, DateTime.UtcNow.AddDays(2));
        var user = CreateUser(command.InitiatorUserId, UserRole.Patient);
        var type = CreateAppointmentType(appointmentTypeId, AppointmentCategory.Checkup);
        var doctor = CreateDoctor(doctorId, specialtyId);
        var specialty = CreateSpecialty(specialtyId, 24);
        var initiatorPatient = CreatePatient(patientId, user.Id);

        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(command.AppointmentId)).ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync(user);
        _appointmentTypeRepositoryMock.Setup(x => x.GetByIdAsync(appointment.AppointmentTypeId)).ReturnsAsync(type);
        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(appointment.DoctorId)).ReturnsAsync(doctor);
        _doctorRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync((Doctor?)null);
        _patientRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(initiatorPatient);
        _specialtyRepositoryMock.Setup(x => x.GetByIdAsync(doctor.MedicalSpecialtyId)).ReturnsAsync(specialty);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _appointmentRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Appointment>(a => a.Status == AppointmentStatus.Cancelled)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToLateCancellation_WhenCancellationIsLate()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), false, "Late cancel reason");

        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();

        // Appointment is scheduled 2 hours from now, which violates 24h minimum notice
        var appointment = CreateAppointment(command.AppointmentId, doctorId, patientId, appointmentTypeId, DateTime.UtcNow.AddHours(2));
        var user = CreateUser(command.InitiatorUserId, UserRole.Patient);
        var type = CreateAppointmentType(appointmentTypeId, AppointmentCategory.Checkup);
        var doctor = CreateDoctor(doctorId, specialtyId);
        var specialty = CreateSpecialty(specialtyId, 24);
        var initiatorPatient = CreatePatient(patientId, user.Id);

        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(command.AppointmentId)).ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync(user);
        _appointmentTypeRepositoryMock.Setup(x => x.GetByIdAsync(appointment.AppointmentTypeId)).ReturnsAsync(type);
        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(appointment.DoctorId)).ReturnsAsync(doctor);
        _doctorRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync((Doctor?)null);
        _patientRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(initiatorPatient);
        _specialtyRepositoryMock.Setup(x => x.GetByIdAsync(doctor.MedicalSpecialtyId)).ReturnsAsync(specialty);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _appointmentRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Appointment>(a => a.Status == AppointmentStatus.LateCancellation)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), false, "Reason");
        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(command.AppointmentId)).ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage($"*Appointment*");
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), false, "Reason");
        var appointment = CreateAppointment(command.AppointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(2));

        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(command.AppointmentId)).ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage($"*User*");
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeNotFound()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), false, "Reason");
        var appointment = CreateAppointment(command.AppointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(2));
        var user = CreateUser(command.InitiatorUserId, UserRole.Patient);

        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(command.AppointmentId)).ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync(user);
        _appointmentTypeRepositoryMock.Setup(x => x.GetByIdAsync(appointment.AppointmentTypeId)).ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage($"*AppointmentTypeDefinition*");
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), false, "Reason");
        var appointment = CreateAppointment(command.AppointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(2));
        var user = CreateUser(command.InitiatorUserId, UserRole.Patient);
        var type = CreateAppointmentType(appointment.AppointmentTypeId, AppointmentCategory.Checkup);

        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(command.AppointmentId)).ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync(user);
        _appointmentTypeRepositoryMock.Setup(x => x.GetByIdAsync(appointment.AppointmentTypeId)).ReturnsAsync(type);
        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(appointment.DoctorId)).ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage($"*Doctor*");
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenSpecialtyNotFound()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), false, "Reason");
        var specialtyId = Guid.NewGuid();
        var appointment = CreateAppointment(command.AppointmentId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(2));
        var user = CreateUser(command.InitiatorUserId, UserRole.Patient);
        var type = CreateAppointmentType(appointment.AppointmentTypeId, AppointmentCategory.Checkup);
        var doctor = CreateDoctor(appointment.DoctorId, specialtyId);

        _appointmentRepositoryMock.Setup(x => x.GetByIdAsync(command.AppointmentId)).ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync(user);
        _appointmentTypeRepositoryMock.Setup(x => x.GetByIdAsync(appointment.AppointmentTypeId)).ReturnsAsync(type);
        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(appointment.DoctorId)).ReturnsAsync(doctor);
        _specialtyRepositoryMock.Setup(x => x.GetByIdAsync(specialtyId)).ReturnsAsync((MedicalSpecialty?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage($"*MedicalSpecialty*");
    }

    // Helpers
    private static Appointment CreateAppointment(Guid id, Guid doctorId, Guid patientId, Guid typeId, DateTime scheduledDateTime)
    {
        var appointment = Appointment.Schedule(patientId, doctorId, typeId, scheduledDateTime.Date, TimeRange.Create(scheduledDateTime.TimeOfDay,
            scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))));
        SetPrivateProperty(appointment, nameof(Appointment.Id), id);
        return appointment;
    }

    private static User CreateUser(Guid id, UserRole role)
    {
        var user = User.Create(EmailAddress.Create("test@clinic.com"), "hashedpassword", PersonName.Create("Test User"), PhoneNumber.Create("555-0000"), role);
        SetPrivateProperty(user, nameof(User.Id), id);
        return user;
    }

    private static AppointmentTypeDefinition CreateAppointmentType(Guid id, AppointmentCategory typeEnum)
    {
        var type = AppointmentTypeDefinition.Create(typeEnum, typeEnum.ToString(), "Test description", TimeSpan.FromMinutes(30));
        SetPrivateProperty(type, nameof(AppointmentTypeDefinition.Id), id);
        return type;
    }

    private static Doctor CreateDoctor(Guid id, Guid specialtyId)
    {
        var doctor = Doctor.Create(Guid.NewGuid(), MedicalLicenseNumber.Create("12345"), specialtyId, "Room 1", 10);
        SetPrivateProperty(doctor, nameof(Doctor.Id), id);
        return doctor;
    }

    private static Patient CreatePatient(Guid id, Guid userId)
    {
        var patient = Patient.Create(userId, DateTime.UtcNow.AddYears(-30), BloodType.Create("O+"), "None", "None",
            EmergencyContact.Create("Emergency Contact", "555-9999"));
        SetPrivateProperty(patient, nameof(Patient.Id), id);
        return patient;
    }

    private static MedicalSpecialty CreateSpecialty(Guid id, int minCancellationHours)
    {
        var specialty = (MedicalSpecialty)Activator.CreateInstance(typeof(MedicalSpecialty), true)!;
        SetPrivateProperty(specialty, nameof(MedicalSpecialty.Id), id);
        SetPrivateProperty(specialty, nameof(MedicalSpecialty.MinCancellationHours), minCancellationHours);
        return specialty;
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

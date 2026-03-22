using System.Reflection;
using ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShow;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.MarkAppointmentAsNoShow;

public class MarkAppointmentAsNoShowCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MarkAppointmentAsNoShowCommandHandler _sut;

    public MarkAppointmentAsNoShowCommandHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new MarkAppointmentAsNoShowCommandHandler(
            _appointmentRepositoryMock.Object,
            _userRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _penaltyRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Receptionist)]
    public async Task Handle_ShouldMarkAsNoShow_WhenUserIsAdminOrReceptionist(UserRole role)
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowCommand(Guid.NewGuid(), Guid.NewGuid());
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointment = CreateAppointment(
            command.AppointmentId,
            doctorId,
            patientId,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1)
        );
        var user = CreateUser(command.InitiatorUserId, role);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId))
            .ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync(user);
        _doctorRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync((Doctor?)null);
        _penaltyRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(appointment.PatientId))
            .ReturnsAsync([]);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        _appointmentRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Appointment>(a => a.Status == AppointmentStatus.NoShow)),
            Times.Once
        );
        _penaltyRepositoryMock.Verify(
            x =>
                x.AddAsync(
                    It.Is<PatientPenalty>(p =>
                        p.Type == PenaltyType.Warning && p.Reason == "No show"
                    )
                ),
            Times.AtLeastOnce
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMarkAsNoShow_WhenUserIsAssignedDoctor()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowCommand(Guid.NewGuid(), Guid.NewGuid());
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointment = CreateAppointment(
            command.AppointmentId,
            doctorId,
            patientId,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1)
        );
        var user = CreateUser(command.InitiatorUserId, UserRole.Doctor);
        var doctor = CreateDoctor(doctorId, user.Id);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId))
            .ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync(user);
        _doctorRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(doctor);
        _penaltyRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(appointment.PatientId))
            .ReturnsAsync([]);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        _penaltyRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<PatientPenalty>()),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsUnassignedDoctor()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowCommand(Guid.NewGuid(), Guid.NewGuid());
        var doctorId = Guid.NewGuid();
        var anotherDoctorId = Guid.NewGuid();
        var appointment = CreateAppointment(
            command.AppointmentId,
            doctorId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1)
        );
        var user = CreateUser(command.InitiatorUserId, UserRole.Doctor);
        var doctor = CreateDoctor(anotherDoctorId, user.Id);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId))
            .ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync(user);
        _doctorRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync(doctor);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<AppointmentNoShowUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.CannotMarkNoShow);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsPatient()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowCommand(Guid.NewGuid(), Guid.NewGuid());
        var patientId = Guid.NewGuid();
        var appointment = CreateAppointment(
            command.AppointmentId,
            Guid.NewGuid(),
            patientId,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1)
        );
        var user = CreateUser(command.InitiatorUserId, UserRole.Patient);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId))
            .ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync(user);
        _doctorRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<AppointmentNoShowUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.CannotMarkNoShow);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFound_WhenAppointmentDoesNotExist()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowCommand(Guid.NewGuid(), Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowCommand(Guid.NewGuid(), Guid.NewGuid());
        var appointment = CreateAppointment(
            command.AppointmentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1)
        );

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId))
            .ReturnsAsync(appointment);
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.InitiatorUserId))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserIsDoctorButDoctorProfileIsNull()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowCommand(Guid.NewGuid(), Guid.NewGuid());
        var appointment = CreateAppointment(
            command.AppointmentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1)
        );
        var user = CreateUser(command.InitiatorUserId, UserRole.Doctor);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId))
            .ReturnsAsync(appointment);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(command.InitiatorUserId)).ReturnsAsync(user);
        _doctorRepositoryMock.Setup(x => x.GetByUserIdAsync(user.Id)).ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<AppointmentNoShowUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.CannotMarkNoShow);
    }

    // Helpers
    private static Appointment CreateAppointment(
        Guid id,
        Guid doctorId,
        Guid patientId,
        Guid typeId,
        DateTime scheduledDateTime
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
        return appointment;
    }

    private static User CreateUser(Guid id, UserRole role)
    {
        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword",
            PhoneNumber.Create("555-0000"),
            role
        );
        SetPrivateProperty(user, nameof(User.Id), id);
        return user;
    }

    private static Doctor CreateDoctor(Guid id, Guid specialtyId)
    {
        var doctor = Doctor.Create(
            Guid.NewGuid(),
            MedicalLicenseNumber.Create("12345"),
            specialtyId,
            "Room 1",
            10
        );
        SetPrivateProperty(doctor, nameof(Doctor.Id), id);
        return doctor;
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

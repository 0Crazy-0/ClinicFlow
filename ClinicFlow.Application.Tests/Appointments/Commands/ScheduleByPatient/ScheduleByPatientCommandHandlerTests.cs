using ClinicFlow.Application.Appointments.Commands.ScheduleByPatient;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleByPatient;

public class ScheduleByPatientCommandHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock =
        new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly ScheduleByPatientCommandHandler _sut;

    public ScheduleByPatientCommandHandlerTests()
    {
        _sut = new ScheduleByPatientCommandHandler(
            _penaltyRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _appointmentTypeRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenValidRequest()
    {
        // Arrange
        var scheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date;
        var startTime = new TimeSpan(10, 0, 0);
        var endTime = new TimeSpan(11, 0, 0);

        var command = new ScheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDate,
            startTime,
            endTime
        );

        var targetPatient = CreateTargetPatient(
            command.InitiatorUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(0, 100, false)
        );

        var schedule = Schedule.Create(
            command.DoctorId,
            scheduledDate.DayOfWeek,
            TimeRange.Create(startTime, endTime)
        );

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
        _penaltyRepositoryMock
            .Setup(r =>
                r.GetByPatientIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);
        _scheduleRepositoryMock
            .Setup(r =>
                r.GetByDoctorAndDayAsync(
                    command.DoctorId,
                    scheduledDate.DayOfWeek,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(schedule);
        _appointmentRepositoryMock
            .Setup(r =>
                r.HasConflictAsync(
                    command.DoctorId,
                    scheduledDate,
                    It.IsAny<TimeRange>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        var user = CreateVerifiedUser();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        _appointmentRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        // Arrange
        var command = new ScheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenInitiatorPatientNotFound()
    {
        // Arrange
        var command = new ScheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var targetPatient = CreateTargetPatient(
            command.TargetPatientId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeNotFound()
    {
        // Arrange
        var command = new ScheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var targetPatient = CreateTargetPatient(
            command.TargetPatientId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var initiatorPatient = CreateTargetPatient(
            command.InitiatorUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(initiatorPatient);

        var user = CreateVerifiedUser();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var command = new ScheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var targetPatient = CreateTargetPatient(
            command.TargetPatientId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var initiatorPatient = CreateTargetPatient(
            command.InitiatorUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(initiatorPatient);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(User));
    }

    private static Patient CreateTargetPatient(Guid userId, DateTime referenceTime)
    {
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test Patient"),
            referenceTime.AddYears(-30).Date,
            referenceTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");

        patient.UpdateEmergencyContact(EmergencyContact.Create("Emergency Name", "555-1234567"));

        return patient;
    }

    private static User CreateVerifiedUser()
    {
        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );
        user.MarkPhoneAsVerified(true);
        return user;
    }
}

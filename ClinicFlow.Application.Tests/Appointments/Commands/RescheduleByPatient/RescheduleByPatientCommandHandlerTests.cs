using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByPatient;

public class RescheduleByPatientCommandHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock =
        new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRegionalSchedulingService> _regionalSchedulingServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RescheduleByPatientCommandHandler _sut;

    public RescheduleByPatientCommandHandlerTests()
    {
        _sut = new RescheduleByPatientCommandHandler(
            _appointmentRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _appointmentTypeRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
            _penaltyRepositoryMock.Object,
            _userRepositoryMock.Object,
            _regionalSchedulingServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenValidRequest()
    {
        // Arrange
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var newStartTime = new TimeOnly(10, 0);
        var newEndTime = new TimeOnly(11, 0);

        var command = new RescheduleByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            newDate,
            newStartTime,
            newEndTime,
            "Reschedule notes"
        );

        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var typeId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        var targetPatient = CreatePatient(patientId, command.InitiatorUserId);
        var schedule = Schedule.Create(
            doctorId,
            newDate.DayOfWeek,
            TimeRange.Create(newStartTime, newEndTime)
        );

        var doctor = CreateDoctor();
        var user = CreateVerifiedUser();
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30),
            null
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _patientRepositoryMock
            .Setup(r =>
                r.GetSelfPatientByUserIdAsync(
                    command.InitiatorUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(targetPatient);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
        _regionalSchedulingServiceMock
            .Setup(s => s.EnforceSchedulingRegulations(doctor, targetPatient, appointmentType))
            .Returns(SchedulingClearance.Granted());
        _penaltyRepositoryMock
            .Setup(r => r.GetHistoryByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _scheduleRepositoryMock
            .Setup(r =>
                r.GetActiveByDoctorAndDayAsync(
                    doctorId,
                    newDate.DayOfWeek,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(schedule);
        _appointmentRepositoryMock
            .Setup(r =>
                r.HasConflictAsync(
                    doctorId,
                    newDate,
                    It.IsAny<TimeRange>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        appointment.ScheduledDate.Should().Be(newDate);
        appointment.PatientNotes.Should().Be(command.NewPatientNotes);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
        );

        var appointment = CreateAppointment();

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenInitiatorPatientNotFound()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
        );

        var appointment = CreateAppointment();
        var targetPatient = CreatePatient(appointment.PatientId, Guid.CreateVersion7());

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r =>
                r.GetSelfPatientByUserIdAsync(
                    command.InitiatorUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
        );

        var appointment = CreateAppointment();
        var targetPatient = CreatePatient(appointment.PatientId, command.InitiatorUserId);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r =>
                r.GetSelfPatientByUserIdAsync(
                    command.InitiatorUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(targetPatient);

        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
        );

        var appointment = CreateAppointment();
        var targetPatient = CreatePatient(appointment.PatientId, command.InitiatorUserId);
        var doctor = CreateDoctor();

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r =>
                r.GetSelfPatientByUserIdAsync(
                    command.InitiatorUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(targetPatient);

        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(User));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeNotFound()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
        );

        var appointment = CreateAppointment();
        var targetPatient = CreatePatient(appointment.PatientId, command.InitiatorUserId);
        var doctor = CreateDoctor();
        var user = CreateVerifiedUser();

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r =>
                r.GetSelfPatientByUserIdAsync(
                    command.InitiatorUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(targetPatient);

        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _appointmentTypeRepositoryMock
            .Setup(r =>
                r.GetByIdAsync(appointment.AppointmentTypeId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowAppointmentConflictException_WhenConflictExists()
    {
        // Arrange
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var newStartTime = new TimeOnly(10, 0);
        var newEndTime = new TimeOnly(11, 0);

        var command = new RescheduleByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            newDate,
            newStartTime,
            newEndTime,
            "Reschedule notes"
        );

        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var typeId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        var targetPatient = CreatePatient(patientId, command.InitiatorUserId);
        var doctor = CreateDoctor();
        var user = CreateVerifiedUser();
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30),
            null
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _patientRepositoryMock
            .Setup(r =>
                r.GetSelfPatientByUserIdAsync(
                    command.InitiatorUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(targetPatient);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _appointmentRepositoryMock
            .Setup(r =>
                r.HasConflictAsync(
                    doctorId,
                    newDate,
                    It.IsAny<TimeRange>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        _scheduleRepositoryMock
            .Setup(r =>
                r.GetActiveByDoctorAndDayAsync(
                    doctorId,
                    newDate.DayOfWeek,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Schedule.Create(
                    doctorId,
                    newDate.DayOfWeek,
                    TimeRange.Create(command.NewStartTime, command.NewEndTime)
                )
            );

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenScheduleNotFound()
    {
        // Arrange
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));

        var command = new RescheduleByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            newDate,
            new TimeOnly(10, 0),
            new TimeOnly(11, 0)
        );

        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var typeId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        var targetPatient = CreatePatient(patientId, command.InitiatorUserId);
        var doctor = CreateDoctor();
        var user = CreateVerifiedUser();
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30),
            null
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _patientRepositoryMock
            .Setup(r =>
                r.GetSelfPatientByUserIdAsync(
                    command.InitiatorUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(targetPatient);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _scheduleRepositoryMock
            .Setup(r =>
                r.GetActiveByDoctorAndDayAsync(
                    doctorId,
                    newDate.DayOfWeek,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Schedule?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Schedule));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

    private static Doctor CreateDoctor() =>
        Doctor.Create(
            Guid.CreateVersion7(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("1234567"),
            Guid.CreateVersion7(),
            "555-1234",
            ConsultationRoom.Create(1, "Room A", 1)
        );

    private Appointment CreateAppointment() =>
        Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

    private Patient CreatePatient(Guid id, Guid userId)
    {
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.SetId(id);

        return patient;
    }
}

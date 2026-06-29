using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.RescheduleByDoctor;
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

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByDoctor;

public class RescheduleByDoctorCommandHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock =
        new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IRegionalSchedulingService> _regionalSchedulingServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RescheduleByDoctorCommandHandler _sut;

    public RescheduleByDoctorCommandHandlerTests()
    {
        _sut = new RescheduleByDoctorCommandHandler(
            _appointmentRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _appointmentTypeRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
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
        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            newDate,
            newStartTime,
            newEndTime,
            false
        );

        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var appointment = CreateAppointment(patientId, doctorId, typeId);
        var doctor = CreateDoctor(doctorId, command.InitiatorUserId);
        var schedule = Schedule.Create(
            doctorId,
            newDate.DayOfWeek,
            TimeRange.Create(newStartTime, newEndTime)
        );

        var targetPatient = CreatePatient();
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
        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
        _regionalSchedulingServiceMock
            .Setup(s => s.EnforceSchedulingRegulations(doctor, targetPatient, appointmentType))
            .Returns(SchedulingClearance.Granted());
        _scheduleRepositoryMock
            .Setup(r =>
                r.GetByDoctorAndDayAsync(doctorId, newDate.DayOfWeek, It.IsAny<CancellationToken>())
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

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        appointment.ScheduledDate.Should().Be(newDate);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            false
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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            false
        );

        var appointment = CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        // Arrange
        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            false
        );

        var doctorId = Guid.NewGuid();
        var appointment = CreateAppointment(Guid.NewGuid(), doctorId, Guid.NewGuid());
        var doctor = CreateDoctor(doctorId, command.InitiatorUserId);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeNotFound()
    {
        // Arrange
        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            false
        );

        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointment = CreateAppointment(patientId, doctorId, Guid.NewGuid());
        var doctor = CreateDoctor(doctorId, command.InitiatorUserId);
        var targetPatient = CreatePatient();

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
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
        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            false
        );

        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var appointment = CreateAppointment(patientId, doctorId, typeId);
        var doctor = CreateDoctor(doctorId, command.InitiatorUserId);
        var targetPatient = CreatePatient();
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
        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
        _appointmentRepositoryMock
            .Setup(r =>
                r.HasConflictAsync(
                    doctorId,
                    command.NewDate,
                    It.IsAny<TimeRange>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        _scheduleRepositoryMock
            .Setup(r =>
                r.GetByDoctorAndDayAsync(
                    doctorId,
                    command.NewDate.DayOfWeek,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Schedule.Create(
                    doctorId,
                    command.NewDate.DayOfWeek,
                    TimeRange.Create(command.NewStartTime, command.NewEndTime)
                )
            );

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

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
        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            false
        );

        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        var appointment = CreateAppointment(patientId, doctorId, typeId);

        var doctor = CreateDoctor(doctorId, command.InitiatorUserId);
        var targetPatient = CreatePatient();
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
        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
        _scheduleRepositoryMock
            .Setup(r =>
                r.GetByDoctorAndDayAsync(
                    doctorId,
                    command.NewDate.DayOfWeek,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Schedule?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Schedule));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Appointment CreateAppointment(Guid patientId, Guid doctorId, Guid typeId) =>
        Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

    private static Doctor CreateDoctor(Guid id, Guid userId)
    {
        var doctor = Doctor.Create(
            userId,
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("1234567"),
            Guid.NewGuid(),
            "555-1234",
            ConsultationRoom.Create(1, "Room A", 1)
        );

        doctor.SetId(id);

        return doctor;
    }

    private Patient CreatePatient() =>
        Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
}

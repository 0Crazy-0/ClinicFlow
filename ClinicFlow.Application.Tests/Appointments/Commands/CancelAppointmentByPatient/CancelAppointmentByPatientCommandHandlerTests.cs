using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByPatient;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.CancelAppointmentByPatient;

public class CancelAppointmentByPatientCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock =
        new();
    private readonly Mock<IMedicalSpecialtyRepository> _specialtyRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly CancelAppointmentByPatientCommandHandler _sut;

    public CancelAppointmentByPatientCommandHandlerTests()
    {
        _sut = new CancelAppointmentByPatientCommandHandler(
            _fakeTime,
            _appointmentRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _appointmentTypeRepositoryMock.Object,
            _specialtyRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenValidRequest()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Reason"
        );

        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var specialtyId = Guid.CreateVersion7();
        var typeId = Guid.CreateVersion7();
        var appointment = CreateAppointment(patientId, doctorId, typeId);
        var patient = CreatePatient(patientId, command.InitiatorUserId);
        var typeDef = CreateAppointmentType();
        var doctor = CreateDoctor(command.InitiatorUserId, specialtyId);
        var specialty = MedicalSpecialty.Create("Test Specialty", "Test Description", 30, 24);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(typeDef);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _specialtyRepositoryMock
            .Setup(r => r.GetByIdAsync(specialtyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(specialty);
        _patientRepositoryMock
            .Setup(r => r.GetSelfPatientByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Reason"
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
        var command = new CancelAppointmentByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Reason"
        );

        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var typeId = Guid.CreateVersion7();
        var appointment = CreateAppointment(patientId, doctorId, typeId);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
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
        var command = new CancelAppointmentByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Reason"
        );

        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var typeId = Guid.CreateVersion7();
        var appointment = CreateAppointment(patientId, doctorId, typeId);
        var patient = CreatePatient(patientId, command.InitiatorUserId);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Reason"
        );

        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var typeId = Guid.CreateVersion7();
        var appointment = CreateAppointment(patientId, doctorId, typeId);
        var patient = CreatePatient(patientId, command.InitiatorUserId);
        var typeDef = CreateAppointmentType();

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(typeDef);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenSpecialtyNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Reason"
        );

        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var specialtyId = Guid.CreateVersion7();
        var typeId = Guid.CreateVersion7();
        var appointment = CreateAppointment(patientId, doctorId, typeId);
        var patient = CreatePatient(patientId, command.InitiatorUserId);
        var typeDef = CreateAppointmentType();
        var doctor = CreateDoctor(command.InitiatorUserId, specialtyId);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(typeDef);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _specialtyRepositoryMock
            .Setup(r => r.GetByIdAsync(specialtyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalSpecialty?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalSpecialty));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Appointment CreateAppointment(Guid patientId, Guid doctorId, Guid typeId) =>
        Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
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

    private static AppointmentTypeDefinition CreateAppointmentType() =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30)
        );

    private static Doctor CreateDoctor(Guid userId, Guid specialtyId) =>
        Doctor.Create(
            userId,
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("1234567"),
            specialtyId,
            "555-1234",
            ConsultationRoom.Create(1, "Room A", 1)
        );
}

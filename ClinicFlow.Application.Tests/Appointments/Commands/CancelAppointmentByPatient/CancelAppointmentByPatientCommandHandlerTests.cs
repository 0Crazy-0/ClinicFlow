using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByPatient;
using ClinicFlow.Application.Tests.Shared;
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
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason"
        );

        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        var appointment = CreateAppointment(
            command.AppointmentId,
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patient = CreatePatient(
            patientId,
            command.InitiatorUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var typeDef = CreateAppointmentType(typeId);
        var doctor = CreateDoctor(doctorId, command.InitiatorUserId, specialtyId);
        var specialty = CreateSpecialty(specialtyId);

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
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _appointmentRepositoryMock.Verify(
            r => r.UpdateAsync(appointment, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason"
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason"
        );

        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        var appointment = CreateAppointment(
            command.AppointmentId,
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<EntityNotFoundException>();
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason"
        );

        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        var appointment = CreateAppointment(
            command.AppointmentId,
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patient = CreatePatient(
            patientId,
            command.InitiatorUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

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
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<EntityNotFoundException>();
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason"
        );

        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        var appointment = CreateAppointment(
            command.AppointmentId,
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patient = CreatePatient(
            patientId,
            command.InitiatorUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var typeDef = CreateAppointmentType(typeId);

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
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<EntityNotFoundException>();
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenSpecialtyNotFound()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason"
        );

        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var specialtyId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        var appointment = CreateAppointment(
            command.AppointmentId,
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patient = CreatePatient(
            patientId,
            command.InitiatorUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var typeDef = CreateAppointmentType(typeId);
        var doctor = CreateDoctor(doctorId, command.InitiatorUserId, specialtyId);

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
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<EntityNotFoundException>();
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalSpecialty));
    }

    private static Appointment CreateAppointment(
        Guid id,
        Guid patientId,
        Guid doctorId,
        Guid typeId,
        DateTime referenceTime
    )
    {
        var scheduledDate = referenceTime.AddDays(2).Date;
        var timeRange = TimeRange.Create(new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            scheduledDate,
            timeRange
        );
        appointment.SetId(id);
        return appointment;
    }

    private static Patient CreatePatient(Guid id, Guid userId, DateTime referenceTime)
    {
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test"),
            referenceTime.AddYears(-30),
            referenceTime
        );
        patient.SetId(id);
        return patient;
    }

    private static AppointmentTypeDefinition CreateAppointmentType(Guid id)
    {
        var typeDef = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            TimeSpan.FromMinutes(30)
        );
        typeDef.SetId(id);
        return typeDef;
    }

    private static Doctor CreateDoctor(Guid id, Guid userId, Guid specialtyId)
    {
        var doctor = Doctor.Create(
            userId,
            MedicalLicenseNumber.Create("1234567"),
            specialtyId,
            "555-1234",
            101
        );
        doctor.SetId(id);
        return doctor;
    }

    private static MedicalSpecialty CreateSpecialty(Guid id, int minCancellationHours = 24)
    {
        var specialty = MedicalSpecialty.Create(
            "Test Specialty",
            "Test Description",
            30,
            minCancellationHours
        );
        specialty.SetId(id);
        return specialty;
    }
}

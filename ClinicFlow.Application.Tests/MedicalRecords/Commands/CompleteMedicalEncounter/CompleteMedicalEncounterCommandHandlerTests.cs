using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Commands.CompleteMedicalEncounter;

public class CompleteMedicalEncounterCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly MedicalEncounterService _medicalEncounterService;
    private readonly CompleteMedicalEncounterCommandHandler _sut;

    public CompleteMedicalEncounterCommandHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // Use a real MedicalEncounterService with empty policies since it's a domain service
        var jsonValidatorMock = new Mock<IJsonSchemaValidator>();
        _medicalEncounterService = new MedicalEncounterService([], jsonValidatorMock.Object);

        _sut = new CompleteMedicalEncounterCommandHandler(
            _doctorRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _appointmentTypeRepositoryMock.Object,
            _medicalRecordRepositoryMock.Object,
            _medicalEncounterService,
            _unitOfWorkMock.Object,
            _fakeTime
        );
    }

    [Fact]
    public async Task Handle_ShouldCompleteMedicalEncounter_WhenAllEntitiesExistAndValid()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();
        var command = new CompleteMedicalEncounterCommand(
            patientId,
            doctorId,
            appointmentId,
            "Headache",
            [new DynamicClinicalDetailDto("vital-signs", "{}")]
        );

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateAppointment(
            appointmentId,
            appointmentTypeId,
            patientId,
            doctorId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(0, 100, false)
        );

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);
        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentTypeId))
            .ReturnsAsync(appointmentType);

        MedicalRecord? capturedRecord = null;
        _medicalRecordRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<MedicalRecord>(), It.IsAny<CancellationToken>()))
            .Callback<MedicalRecord, CancellationToken>((r, _) => capturedRecord = r);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedRecord.Should().NotBeNull();
        capturedRecord.PatientId.Should().Be(patientId);
        capturedRecord.DoctorId.Should().Be(doctorId);
        capturedRecord.AppointmentId.Should().Be(appointmentId);
        capturedRecord.ChiefComplaint.Should().Be(command.ChiefComplaint);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenAllEntitiesExistAndValid()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();
        var command = new CompleteMedicalEncounterCommand(
            patientId,
            doctorId,
            appointmentId,
            "Headache",
            [new DynamicClinicalDetailDto("vital-signs", "{}")]
        );

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateAppointment(
            appointmentId,
            appointmentTypeId,
            patientId,
            doctorId,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(0, 100, false)
        );

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);
        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentTypeId))
            .ReturnsAsync(appointmentType);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _medicalRecordRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<MedicalRecord>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorDoesNotExist()
    {
        // Arrange
        var command = new CompleteMedicalEncounterCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Headache",
            []
        );

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(command.DoctorId))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));

        _medicalRecordRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<MedicalRecord>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentDoesNotExist()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var command = new CompleteMedicalEncounterCommand(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            "Headache",
            []
        );

        var doctor = CreateDoctor(doctorId);
        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));

        _medicalRecordRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<MedicalRecord>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeDoesNotExist()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var appointmentTypeId = Guid.NewGuid();
        var command = new CompleteMedicalEncounterCommand(
            patientId,
            doctorId,
            appointmentId,
            "Headache",
            []
        );

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateAppointment(
            appointmentId,
            appointmentTypeId,
            patientId,
            doctorId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);
        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentTypeId))
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));

        _medicalRecordRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<MedicalRecord>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Doctor CreateDoctor(Guid id)
    {
        var doctor = Doctor.Create(
            Guid.NewGuid(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("RM-12345"),
            Guid.NewGuid(),
            "Bio",
            ConsultationRoom.Create(1, "Room A", 1)
        );
        doctor.SetId(id);
        return doctor;
    }

    private static Appointment CreateAppointment(
        Guid id,
        Guid appointmentTypeId,
        Guid patientId,
        Guid doctorId,
        DateTime dt
    )
    {
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            appointmentTypeId,
            DateOnly.FromDateTime(dt.AddDays(1)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );
        appointment.SetId(id);
        appointment.CheckIn(dt);
        appointment.Start(doctorId, dt);

        return appointment;
    }
}

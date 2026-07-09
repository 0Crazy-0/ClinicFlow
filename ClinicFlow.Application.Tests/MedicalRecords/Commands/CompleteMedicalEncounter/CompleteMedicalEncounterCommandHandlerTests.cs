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
        var doctorId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var command = new CompleteMedicalEncounterCommand(
            patientId,
            doctorId,
            appointmentId,
            "Headache",
            [new DynamicClinicalDetailDto("vital-signs", "{}")]
        );

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateAppointment(appointmentId, appointmentTypeId, patientId, doctorId);

        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(0, 100, false)
        );

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);

        MedicalRecord? capturedRecord = null;
        _medicalRecordRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<MedicalRecord>(), It.IsAny<CancellationToken>()))
            .Callback<MedicalRecord, CancellationToken>((r, _) => capturedRecord = r);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

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
        var doctorId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var command = new CompleteMedicalEncounterCommand(
            patientId,
            doctorId,
            appointmentId,
            "Headache",
            [new DynamicClinicalDetailDto("vital-signs", "{}")]
        );

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateAppointment(appointmentId, appointmentTypeId, patientId, doctorId);

        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(0, 100, false)
        );

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

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
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Headache",
            []
        );

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

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
        var doctorId = Guid.CreateVersion7();
        var command = new CompleteMedicalEncounterCommand(
            Guid.CreateVersion7(),
            doctorId,
            Guid.CreateVersion7(),
            "Headache",
            []
        );

        var doctor = CreateDoctor(doctorId);
        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

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
        var doctorId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var command = new CompleteMedicalEncounterCommand(
            patientId,
            doctorId,
            appointmentId,
            "Headache",
            []
        );

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateAppointment(appointmentId, appointmentTypeId, patientId, doctorId);

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

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
            Guid.CreateVersion7(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("RM-12345"),
            Guid.CreateVersion7(),
            "Bio",
            ConsultationRoom.Create(1, "Room A", 1)
        );

        doctor.SetId(id);

        return doctor;
    }

    private Appointment CreateAppointment(
        Guid id,
        Guid appointmentTypeId,
        Guid patientId,
        Guid doctorId
    )
    {
        var referenceDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1);
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            appointmentTypeId,
            DateOnly.FromDateTime(referenceDate.AddDays(1)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        appointment.SetId(id);
        appointment.CheckIn(DateOnly.FromDateTime(referenceDate));
        appointment.Start(doctorId, referenceDate);

        return appointment;
    }
}

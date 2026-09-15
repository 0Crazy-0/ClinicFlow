using AwesomeAssertions;
using ClinicFlow.Application.MedicalRecords.Commands.StartMedicalEncounter;
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

namespace ClinicFlow.Application.Tests.MedicalRecords.Commands.StartMedicalEncounter;

public class StartMedicalEncounterCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly StartMedicalEncounterCommandHandler _sut;

    public StartMedicalEncounterCommandHandlerTests()
    {
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _appointmentTypeRepositoryMock = new Mock<IAppointmentTypeDefinitionRepository>();
        _medicalRecordRepositoryMock = new Mock<IMedicalRecordRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new StartMedicalEncounterCommandHandler(
            _doctorRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _appointmentTypeRepositoryMock.Object,
            _medicalRecordRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fakeTime
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateRecordWithProtection_WhenTypeHasProtectedCategory()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var command = new StartMedicalEncounterCommand(doctorId, appointmentId, "Headache", true);

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateCheckedInAppointment(appointmentId, appointmentTypeId, doctorId);

        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(0, 100, false),
            ProtectedCategory.SubstanceAbuseTreatment
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
        capturedRecord.PatientId.Should().Be(appointment.PatientId);
        capturedRecord.DoctorId.Should().Be(doctorId);
        capturedRecord.AppointmentId.Should().Be(appointmentId);
        capturedRecord.ChiefComplaint.Should().Be(command.ChiefComplaint);
        capturedRecord.ProtectedCareCategory.Should().Be(ProtectedCategory.SubstanceAbuseTreatment);
        capturedRecord.GuardianInitiatedTreatment.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
    }

    [Fact]
    public async Task Handle_ShouldCreateRecordWithoutProtection_WhenTypeHasNoProtectedCategory()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var command = new StartMedicalEncounterCommand(doctorId, appointmentId, "Headache", null);

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateCheckedInAppointment(appointmentId, appointmentTypeId, doctorId);

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
        capturedRecord.PatientId.Should().Be(appointment.PatientId);
        capturedRecord.DoctorId.Should().Be(doctorId);
        capturedRecord.AppointmentId.Should().Be(appointmentId);
        capturedRecord.ChiefComplaint.Should().Be(command.ChiefComplaint);
        capturedRecord.ProtectedCareCategory.Should().BeNull();
        capturedRecord.GuardianInitiatedTreatment.Should().BeNull();
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var command = new StartMedicalEncounterCommand(doctorId, appointmentId, "Headache", null);

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateCheckedInAppointment(appointmentId, appointmentTypeId, doctorId);

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
        var command = new StartMedicalEncounterCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Headache",
            null
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
        var command = new StartMedicalEncounterCommand(
            doctorId,
            Guid.CreateVersion7(),
            "Headache",
            null
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
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var command = new StartMedicalEncounterCommand(doctorId, appointmentId, "Headache", null);

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateCheckedInAppointment(appointmentId, appointmentTypeId, doctorId);

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

    private Appointment CreateCheckedInAppointment(Guid id, Guid appointmentTypeId, Guid doctorId)
    {
        var now = _fakeTime.GetUtcNow().UtcDateTime;
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            doctorId,
            appointmentTypeId,
            DateOnly.FromDateTime(now.AddDays(-1)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10))
        );

        appointment.SetId(id);
        appointment.CheckIn(DateOnly.FromDateTime(now.AddDays(-1)));

        return appointment;
    }
}

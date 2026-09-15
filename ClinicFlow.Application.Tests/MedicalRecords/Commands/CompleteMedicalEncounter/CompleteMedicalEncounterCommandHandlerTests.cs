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

        var jsonValidatorMock = new Mock<IJsonSchemaValidator>();

        // Use a real MedicalEncounterService with its validation policy since it's a domain service
        _medicalEncounterService = new MedicalEncounterService(
            new MetadataFormValidationPolicy(jsonValidatorMock.Object),
            jsonValidatorMock.Object
        );

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
    public async Task Handle_ShouldCompleteExistingRecord_WhenDetailsWereAddedBeforehand()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var record = MedicalRecord.Create(
            Guid.CreateVersion7(),
            doctorId,
            appointmentId,
            "Headache",
            null,
            null
        );
        var command = new CompleteMedicalEncounterCommand(doctorId, appointmentId, record.Id);

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateAppointment(appointmentId, appointmentTypeId, doctorId);

        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(0, 100, false)
        );

        var detail = DynamicClinicalDetail.Create("vital-signs", "{}");
        var template = ClinicalFormTemplate.Create("vital-signs", "Vitals", "Desc", "{}");
        _medicalEncounterService.AppendClinicalDetail(record, detail, template);

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(record.Id);
        record.ClinicalDetails.Should().ContainSingle().Which.Should().Be(detail);
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenExistingRecordIsCompleted()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var record = MedicalRecord.Create(
            Guid.CreateVersion7(),
            doctorId,
            appointmentId,
            "Headache",
            null,
            null
        );
        var command = new CompleteMedicalEncounterCommand(doctorId, appointmentId, record.Id);

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateAppointment(appointmentId, appointmentTypeId, doctorId);

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
        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _medicalRecordRepositoryMock.Verify(
            x => x.GetByIdAsync(record.Id, It.IsAny<CancellationToken>()),
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
            Guid.CreateVersion7()
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

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentDoesNotExist()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var command = new CompleteMedicalEncounterCommand(
            doctorId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
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

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeDoesNotExist()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var record = MedicalRecord.Create(
            Guid.CreateVersion7(),
            doctorId,
            appointmentId,
            "Headache",
            null,
            null
        );
        var command = new CompleteMedicalEncounterCommand(doctorId, appointmentId, record.Id);

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateAppointment(appointmentId, appointmentTypeId, doctorId);

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

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMedicalRecordDoesNotExist()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var appointmentTypeId = Guid.CreateVersion7();
        var command = new CompleteMedicalEncounterCommand(
            doctorId,
            appointmentId,
            Guid.CreateVersion7()
        );

        var doctor = CreateDoctor(doctorId);
        var appointment = CreateAppointment(appointmentId, appointmentTypeId, doctorId);

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
        _medicalRecordRepositoryMock
            .Setup(x => x.GetByIdAsync(command.MedicalRecordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MedicalRecord?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(MedicalRecord));

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

    private Appointment CreateAppointment(Guid id, Guid appointmentTypeId, Guid doctorId)
    {
        var referenceDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1);
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            doctorId,
            appointmentTypeId,
            DateOnly.FromDateTime(referenceDate.AddDays(-2)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10))
        );

        appointment.SetId(id);
        appointment.CheckIn(DateOnly.FromDateTime(referenceDate));
        appointment.Start(
            doctorId,
            appointment.ScheduledDate.ToDateTime(appointment.TimeRange.Start)
        );

        return appointment;
    }
}

using System.Reflection;
using ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Policies;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.MedicalRecords.Commands.CompleteMedicalEncounter;

public class CompleteMedicalEncounterCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock;
    private readonly Mock<IMedicalRecordRepository> _medicalRecordRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
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
            _unitOfWorkMock.Object
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
        var appointment = CreateAppointment(appointmentId, appointmentTypeId, patientId, doctorId);
        var appointmentType = CreateAppointmentTypeDefinition(appointmentTypeId);

        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(doctorId)).ReturnsAsync(doctor);
        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);
        _appointmentTypeRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentTypeId))
            .ReturnsAsync(appointmentType);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _medicalRecordRepositoryMock.Verify(
            x =>
                x.CreateAsync(
                    It.Is<MedicalRecord>(m =>
                        m.PatientId == command.PatientId
                        && m.DoctorId == command.DoctorId
                        && m.AppointmentId == command.AppointmentId
                        && m.ChiefComplaint == command.ChiefComplaint
                        && m.Id == result
                    )
                ),
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
        var appointment = CreateAppointment(appointmentId, appointmentTypeId, patientId, doctorId);

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
    }

    private static Doctor CreateDoctor(Guid id)
    {
        var doctor = (Doctor)Activator.CreateInstance(typeof(Doctor), true)!;
        SetPrivateProperty(doctor, nameof(Doctor.Id), id);
        return doctor;
    }

    private static Appointment CreateAppointment(
        Guid id,
        Guid appointmentTypeId,
        Guid patientId,
        Guid doctorId
    )
    {
        var appointment = (Appointment)Activator.CreateInstance(typeof(Appointment), true)!;
        SetPrivateProperty(appointment, nameof(Appointment.Id), id);
        SetPrivateProperty(appointment, nameof(Appointment.AppointmentTypeId), appointmentTypeId);
        SetPrivateProperty(appointment, nameof(Appointment.PatientId), patientId);
        SetPrivateProperty(appointment, nameof(Appointment.DoctorId), doctorId);
        return appointment;
    }

    private static AppointmentTypeDefinition CreateAppointmentTypeDefinition(Guid id)
    {
        var appointmentType = (AppointmentTypeDefinition)
            Activator.CreateInstance(typeof(AppointmentTypeDefinition), true)!;
        SetPrivateProperty(appointmentType, nameof(AppointmentTypeDefinition.Id), id);
        return appointmentType;
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

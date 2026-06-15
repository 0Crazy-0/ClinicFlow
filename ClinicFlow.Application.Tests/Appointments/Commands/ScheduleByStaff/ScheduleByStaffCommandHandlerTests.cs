using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.ScheduleByStaff;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleByStaff;

public class ScheduleByStaffCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock =
        new();
    private readonly Mock<IRegionalSchedulingService> _regionalSchedulingServiceMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly ScheduleByStaffCommandHandler _sut;

    public ScheduleByStaffCommandHandlerTests()
    {
        _sut = new ScheduleByStaffCommandHandler(
            _patientRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _appointmentTypeRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _regionalSchedulingServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateAppointment_WhenValidRequest()
    {
        // Arrange
        var scheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date;
        var startTime = new TimeOnly(10, 0);
        var endTime = new TimeOnly(11, 0);
        var command = new ScheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDate,
            startTime,
            endTime,
            false,
            false
        );

        var targetPatient = CreateTargetPatient(Guid.NewGuid(), _fakeTime.GetUtcNow().UtcDateTime);
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

        var doctor = Doctor.Create(
            Guid.NewGuid(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("LIC-123"),
            Guid.NewGuid(),
            "Bio",
            ConsultationRoom.Create(1, "Room", 1)
        );

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _regionalSchedulingServiceMock
            .Setup(s => s.EnforceSchedulingRegulations(doctor, targetPatient, appointmentType))
            .Returns(SchedulingClearance.Granted());
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
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
        _regionalSchedulingServiceMock
            .Setup(x =>
                x.EnforceSchedulingRegulations(
                    It.IsAny<Doctor>(),
                    It.IsAny<Patient>(),
                    It.IsAny<AppointmentTypeDefinition>()
                )
            )
            .Returns(SchedulingClearance.Granted());

        Appointment? capturedAppointment = null;
        _appointmentRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((a, _) => capturedAppointment = a);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedAppointment.Should().NotBeNull();
        capturedAppointment.DoctorId.Should().Be(command.DoctorId);
        capturedAppointment.PatientId.Should().Be(targetPatient.Id);
        capturedAppointment.AppointmentTypeId.Should().Be(appointmentType.Id);
        capturedAppointment.ScheduledDate.Should().Be(scheduledDate);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenValidRequest()
    {
        // Arrange
        var scheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date;
        var startTime = new TimeOnly(10, 0);
        var endTime = new TimeOnly(11, 0);
        var command = new ScheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDate,
            startTime,
            endTime,
            false,
            false
        );

        var targetPatient = CreateTargetPatient(Guid.NewGuid(), _fakeTime.GetUtcNow().UtcDateTime);
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

        var doctor = Doctor.Create(
            Guid.NewGuid(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("LIC-123"),
            Guid.NewGuid(),
            "Bio",
            ConsultationRoom.Create(1, "Room", 1)
        );

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _regionalSchedulingServiceMock
            .Setup(s => s.EnforceSchedulingRegulations(doctor, targetPatient, appointmentType))
            .Returns(SchedulingClearance.Granted());
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
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
        _regionalSchedulingServiceMock
            .Setup(x =>
                x.EnforceSchedulingRegulations(
                    It.IsAny<Doctor>(),
                    It.IsAny<Patient>(),
                    It.IsAny<AppointmentTypeDefinition>()
                )
            )
            .Returns(SchedulingClearance.Granted());

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _appointmentRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        // Arrange
        var command = new ScheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            false,
            false
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

        _appointmentRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new ScheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            false,
            false
        );

        var targetPatient = CreateTargetPatient(Guid.NewGuid(), _fakeTime.GetUtcNow().UtcDateTime);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));

        _appointmentRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeNotFound()
    {
        // Arrange
        var command = new ScheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            false,
            false
        );

        var targetPatient = CreateTargetPatient(Guid.NewGuid(), _fakeTime.GetUtcNow().UtcDateTime);

        var doctor = Doctor.Create(
            Guid.NewGuid(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("LIC-123"),
            Guid.NewGuid(),
            "Bio",
            ConsultationRoom.Create(1, "Room", 1)
        );

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(command.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

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

        _appointmentRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
}

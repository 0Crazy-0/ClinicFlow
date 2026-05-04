using ClinicFlow.Application.Appointments.Commands.RescheduleByStaff;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByStaff;

public class RescheduleByStaffCommandHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock =
        new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IRegionalSchedulingService> _regionalSchedulingServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RescheduleByStaffCommandHandler _sut;

    public RescheduleByStaffCommandHandlerTests()
    {
        _sut = new RescheduleByStaffCommandHandler(
            _appointmentRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _doctorRepositoryMock.Object,
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
        var newDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date;
        var newStartTime = new TimeSpan(10, 0, 0);
        var newEndTime = new TimeSpan(11, 0, 0);

        var command = new RescheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            newDate,
            newStartTime,
            newEndTime,
            false
        );

        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var appointment = CreateAppointment(patientId, doctorId, Guid.NewGuid());
        var schedule = CreateSchedule(doctorId, newDate.DayOfWeek, newStartTime, newEndTime);
        var targetPatient = CreatePatient();
        var doctor = CreateDoctor();

        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Desc",
            TimeSpan.FromMinutes(30),
            null
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _appointmentTypeRepositoryMock
            .Setup(r =>
                r.GetByIdAsync(appointment.AppointmentTypeId, It.IsAny<CancellationToken>())
            )
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
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        appointment.ScheduledDate.Should().Be(newDate);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new RescheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false
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
        var command = new RescheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false
        );

        var appointment = CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new RescheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false
        );

        var appointment = CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var targetPatient = CreatePatient();

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.DoctorId, It.IsAny<CancellationToken>()))
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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeNotFound()
    {
        // Arrange
        var command = new RescheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false
        );

        var appointment = CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var targetPatient = CreatePatient();
        var doctor = CreateDoctor();

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.DoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _appointmentTypeRepositoryMock
            .Setup(r =>
                r.GetByIdAsync(appointment.AppointmentTypeId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));
    }

    private Appointment CreateAppointment(Guid patientId, Guid doctorId, Guid typeId) =>
        Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange.Create(new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0))
        );

    private static Doctor CreateDoctor()
    {
        var doctor = Doctor.Create(
            Guid.NewGuid(),
            MedicalLicenseNumber.Create("1234567"),
            Guid.NewGuid(),
            "555-1234",
            ConsultationRoom.Create(1, "Room A", 1)
        );
        return doctor;
    }

    private Patient CreatePatient()
    {
        var patient = Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("Test"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        return patient;
    }

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        TimeSpan start,
        TimeSpan end
    ) => Schedule.Create(doctorId, dayOfWeek, TimeRange.Create(start, end));
}

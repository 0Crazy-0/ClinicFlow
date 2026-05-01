using ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByPatient;

public class RescheduleByPatientCommandHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RescheduleByPatientCommandHandler _sut;

    public RescheduleByPatientCommandHandlerTests()
    {
        _sut = new RescheduleByPatientCommandHandler(
            _appointmentRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
            _penaltyRepositoryMock.Object,
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

        var command = new RescheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            newDate,
            newStartTime,
            newEndTime
        );

        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var appointment = CreateAppointment(patientId, doctorId, Guid.NewGuid());
        var targetPatient = CreatePatient(patientId, command.InitiatorUserId);
        var schedule = CreateSchedule(doctorId, newDate.DayOfWeek, newStartTime, newEndTime);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _penaltyRepositoryMock
            .Setup(r => r.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
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
        _appointmentRepositoryMock.Verify(
            r => r.UpdateAsync(appointment, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        appointment.ScheduledDate.Should().Be(newDate);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
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
        var command = new RescheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenInitiatorPatientNotFound()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var appointment = CreateAppointment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var targetPatient = CreatePatient(appointment.PatientId, Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));
    }

    private Appointment CreateAppointment(Guid patientId, Guid doctorId, Guid typeId) =>
        Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange.Create(new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0))
        );

    private Patient CreatePatient(Guid id, Guid userId)
    {
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.SetId(id);
        return patient;
    }

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        TimeSpan start,
        TimeSpan end
    ) => Schedule.Create(doctorId, dayOfWeek, TimeRange.Create(start, end));
}

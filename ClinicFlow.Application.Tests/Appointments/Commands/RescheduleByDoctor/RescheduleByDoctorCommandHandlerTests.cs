using System.Reflection;
using ClinicFlow.Application.Appointments.Commands.RescheduleByDoctor;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByDoctor;

public class RescheduleByDoctorCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RescheduleByDoctorCommandHandler _sut;

    public RescheduleByDoctorCommandHandlerTests()
    {
        _sut = new RescheduleByDoctorCommandHandler(
            _appointmentRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenValidRequest()
    {
        // Arrange
        var newDate = DateTime.UtcNow.AddDays(1).Date;
        var newStartTime = new TimeSpan(10, 0, 0);
        var newEndTime = new TimeSpan(11, 0, 0);

        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            newDate,
            newStartTime,
            newEndTime,
            false
        );

        var doctorId = Guid.NewGuid();
        var appointment = CreateAppointment(
            command.AppointmentId,
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid()
        );
        var doctor = CreateDoctor(doctorId, command.InitiatorUserId, Guid.NewGuid());
        var schedule = CreateSchedule(
            Guid.NewGuid(),
            doctorId,
            newDate.DayOfWeek,
            newStartTime,
            newEndTime
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
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
        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false
        );

        var appointment = CreateAppointment(
            command.AppointmentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));
    }

    private static Appointment CreateAppointment(
        Guid id,
        Guid patientId,
        Guid doctorId,
        Guid typeId
    )
    {
        var appointment = (Appointment)Activator.CreateInstance(typeof(Appointment), true)!;
        SetPrivateProperty(appointment, nameof(Appointment.Id), id);
        SetPrivateProperty(appointment, nameof(Appointment.PatientId), patientId);
        SetPrivateProperty(appointment, nameof(Appointment.DoctorId), doctorId);
        SetPrivateProperty(appointment, nameof(Appointment.AppointmentTypeId), typeId);
        SetPrivateProperty(appointment, nameof(Appointment.Status), AppointmentStatus.Scheduled);
        return appointment;
    }

    private static Doctor CreateDoctor(Guid id, Guid userId, Guid specialtyId)
    {
        var doctor = (Doctor)Activator.CreateInstance(typeof(Doctor), true)!;
        SetPrivateProperty(doctor, nameof(Doctor.Id), id);
        SetPrivateProperty(doctor, nameof(Doctor.UserId), userId);
        SetPrivateProperty(doctor, nameof(Doctor.MedicalSpecialtyId), specialtyId);
        return doctor;
    }

    private static Schedule CreateSchedule(
        Guid id,
        Guid doctorId,
        DayOfWeek dayOfWeek,
        TimeSpan start,
        TimeSpan end
    )
    {
        var schedule = (Schedule)Activator.CreateInstance(typeof(Schedule), true)!;
        SetPrivateProperty(schedule, nameof(Schedule.Id), id);
        SetPrivateProperty(schedule, nameof(Schedule.DoctorId), doctorId);
        SetPrivateProperty(schedule, nameof(Schedule.DayOfWeek), dayOfWeek);
        SetPrivateProperty(schedule, nameof(Schedule.TimeRange), TimeRange.Create(start, end));
        SetPrivateProperty(schedule, nameof(Schedule.IsActive), true);
        return schedule;
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

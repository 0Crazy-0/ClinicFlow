using System.Reflection;
using ClinicFlow.Application.Appointments.Commands.ScheduleByDoctor;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleByDoctor;

public class ScheduleByDoctorCommandHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock =
        new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ScheduleByDoctorCommandHandler _sut;

    public ScheduleByDoctorCommandHandlerTests()
    {
        _sut = new ScheduleByDoctorCommandHandler(
            _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _appointmentTypeRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenValidRequest()
    {
        var scheduledDate = DateTime.UtcNow.AddDays(1).Date;
        var startTime = new TimeSpan(10, 0, 0);
        var endTime = new TimeSpan(11, 0, 0);

        var command = new ScheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDate,
            startTime,
            endTime,
            false
        );

        var doctor = CreateDoctor(Guid.NewGuid(), command.InitiatorUserId, Guid.NewGuid());
        var targetPatient = CreateTargetPatient(command.TargetPatientId, Guid.NewGuid());
        var appointmentType = CreateAppointmentType(command.AppointmentTypeId);
        var schedule = CreateSchedule(
            Guid.NewGuid(),
            doctor.Id,
            scheduledDate.DayOfWeek,
            startTime,
            endTime
        );

        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
        _scheduleRepositoryMock
            .Setup(r =>
                r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    scheduledDate.DayOfWeek,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(schedule);
        _appointmentRepositoryMock
            .Setup(r =>
                r.HasConflictAsync(
                    doctor.Id,
                    scheduledDate,
                    It.IsAny<TimeRange>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        var result = await _sut.Handle(command, CancellationToken.None);

        _appointmentRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        var command = new ScheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false
        );
        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        var command = new ScheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false
        );

        var doctor = CreateDoctor(Guid.NewGuid(), command.InitiatorUserId, Guid.NewGuid());

        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentTypeNotFound()
    {
        var command = new ScheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false
        );

        var doctor = CreateDoctor(Guid.NewGuid(), command.InitiatorUserId, Guid.NewGuid());
        var targetPatient = CreateTargetPatient(command.TargetPatientId, Guid.NewGuid());

        _doctorRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));
    }

    private static Doctor CreateDoctor(Guid id, Guid userId, Guid specialtyId)
    {
        var doctor = (Doctor)Activator.CreateInstance(typeof(Doctor), true)!;
        SetPrivateProperty(doctor, nameof(Doctor.Id), id);
        SetPrivateProperty(doctor, nameof(Doctor.UserId), userId);
        SetPrivateProperty(doctor, nameof(Doctor.MedicalSpecialtyId), specialtyId);
        return doctor;
    }

    private static Patient CreateTargetPatient(Guid id, Guid userId)
    {
        var patient = (Patient)Activator.CreateInstance(typeof(Patient), true)!;
        SetPrivateProperty(patient, nameof(Patient.Id), id);
        SetPrivateProperty(patient, nameof(Patient.UserId), userId);
        SetPrivateProperty(patient, nameof(Patient.DateOfBirth), DateTime.UtcNow.AddYears(-30));
        return patient;
    }

    private static AppointmentTypeDefinition CreateAppointmentType(Guid id)
    {
        var type = (AppointmentTypeDefinition)
            Activator.CreateInstance(typeof(AppointmentTypeDefinition), true)!;
        SetPrivateProperty(type, nameof(AppointmentTypeDefinition.Id), id);
        SetPrivateProperty(
            type,
            nameof(AppointmentTypeDefinition.Category),
            AppointmentCategory.FollowUp
        );
        var policy = AgeEligibilityPolicy.Create(0, 100, false); // Allow any age
        SetPrivateProperty(type, nameof(AppointmentTypeDefinition.AgePolicy), policy);
        return type;
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

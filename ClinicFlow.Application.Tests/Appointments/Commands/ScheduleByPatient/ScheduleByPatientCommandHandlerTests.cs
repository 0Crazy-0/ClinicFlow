using System.Reflection;
using ClinicFlow.Application.Appointments.Commands.ScheduleByPatient;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleByPatient;

public class ScheduleByPatientCommandHandlerTests
{
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IAppointmentTypeDefinitionRepository> _appointmentTypeRepositoryMock =
        new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ScheduleByPatientCommandHandler _sut;

    public ScheduleByPatientCommandHandlerTests()
    {
        _sut = new ScheduleByPatientCommandHandler(
            _penaltyRepositoryMock.Object,
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

        var command = new ScheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDate,
            startTime,
            endTime
        );

        var targetPatient = CreateTargetPatient(command.TargetPatientId, command.InitiatorUserId);
        var appointmentType = CreateAppointmentType(command.AppointmentTypeId);
        var schedule = CreateSchedule(
            Guid.NewGuid(),
            command.DoctorId,
            scheduledDate.DayOfWeek,
            startTime,
            endTime
        );

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentType);
        _penaltyRepositoryMock
            .Setup(r =>
                r.GetByPatientIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);
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

        var result = await _sut.Handle(command, CancellationToken.None);

        _appointmentRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        var command = new ScheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );
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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenInitiatorPatientNotFound()
    {
        var command = new ScheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var targetPatient = CreateTargetPatient(command.TargetPatientId, Guid.NewGuid());

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
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
        var command = new ScheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var targetPatient = CreateTargetPatient(command.TargetPatientId, Guid.NewGuid());
        var initiatorPatient = CreateTargetPatient(Guid.NewGuid(), command.InitiatorUserId);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetPatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(initiatorPatient);

        _appointmentTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentTypeDefinition?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(AppointmentTypeDefinition));
    }

    private static Patient CreateTargetPatient(Guid id, Guid userId)
    {
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test Patient"),
            DateTime.UtcNow.AddYears(-30)
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");

        patient.UpdateEmergencyContact(EmergencyContact.Create("Emergency Name", "555-1234567"));

        SetPrivateProperty(patient, nameof(Patient.Id), id);

        return patient;
    }

    private static AppointmentTypeDefinition CreateAppointmentType(Guid id)
    {
        var type = (AppointmentTypeDefinition)
            Activator.CreateInstance(typeof(AppointmentTypeDefinition), true)!;
        SetPrivateProperty(type, nameof(AppointmentTypeDefinition.Id), id);
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

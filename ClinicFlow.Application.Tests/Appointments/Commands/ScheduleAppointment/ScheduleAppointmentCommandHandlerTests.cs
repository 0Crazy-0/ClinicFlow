using System.Reflection;
using ClinicFlow.Application.Appointments.Commands.ScheduleAppointment;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleAppointment;

public class ScheduleAppointmentCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IPatientPenaltyRepository> _penaltyRepositoryMock;
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ScheduleAppointmentCommandHandler _sut;

    public ScheduleAppointmentCommandHandlerTests()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _penaltyRepositoryMock = new Mock<IPatientPenaltyRepository>();
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new ScheduleAppointmentCommandHandler(_patientRepositoryMock.Object, _doctorRepositoryMock.Object, _penaltyRepositoryMock.Object,
        _scheduleRepositoryMock.Object, _appointmentRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldScheduleAppointment_WhenAllEntitiesExistAndValid()
    {
        // Arrange
        var scheduledDate =  DateTime.UtcNow.Date.AddDays(1);

        while (scheduledDate.DayOfWeek == DayOfWeek.Sunday || scheduledDate.DayOfWeek == DayOfWeek.Saturday) 
            scheduledDate = scheduledDate.AddDays(1);


        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), scheduledDate, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));

        var patient = CreatePatient(command.PatientId);
        var doctor = CreateDoctor(command.DoctorId, Guid.NewGuid());
        var schedule = CreateSchedule(command.DoctorId, scheduledDate.DayOfWeek, new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));

        _patientRepositoryMock.Setup(x => x.GetByIdAsync(command.PatientId)).ReturnsAsync(patient);
        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(command.DoctorId)).ReturnsAsync(doctor);
        _penaltyRepositoryMock.Setup(x => x.GetByPatientIdAsync(command.PatientId)).ReturnsAsync([]);
        _scheduleRepositoryMock.Setup(x => x.GetByDoctorAndDayAsync(command.DoctorId, command.ScheduledDate.DayOfWeek)).ReturnsAsync(schedule);
        _appointmentRepositoryMock.Setup(x => x.HasConflictAsync(command.DoctorId, command.ScheduledDate, It.IsAny<TimeRange>())).ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        
        _appointmentRepositoryMock.Verify(x => x.CreateAsync(It.Is<Appointment>(a => a.PatientId == command.PatientId && a.DoctorId == command.DoctorId)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));

        _patientRepositoryMock.Setup(x => x.GetByIdAsync(command.PatientId)).ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage($"*Patient*");
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenDoctorNotFound()
    {
        // Arrange
        var command = new ScheduleAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
        var patient = CreatePatient(command.PatientId);

        _patientRepositoryMock.Setup(x => x.GetByIdAsync(command.PatientId)).ReturnsAsync(patient);
        _doctorRepositoryMock.Setup(x => x.GetByIdAsync(command.DoctorId)).ReturnsAsync((Doctor?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage($"*Doctor*");
    }

    // Helpers
    private static Patient CreatePatient(Guid id)
    {
        var patient = (Patient)Activator.CreateInstance(typeof(Patient), true)!;
        SetPrivateProperty(patient, "Id", id);
        return patient;
    }

    private static Doctor CreateDoctor(Guid id, Guid specialtyId)
    {
        var doctor = Doctor.Create(Guid.NewGuid(), MedicalLicenseNumber.Create("12345"), specialtyId, "Room 1", 10);
        SetPrivateProperty(doctor, "Id", id);
        return doctor;
    }

    private static Schedule CreateSchedule(Guid doctorId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        var schedule = (Schedule)Activator.CreateInstance(typeof(Schedule), true)!;
        SetPrivateProperty(schedule, nameof(Schedule.DoctorId), doctorId);
        SetPrivateProperty(schedule, nameof(Schedule.DayOfWeek), dayOfWeek);
        SetPrivateProperty(schedule, nameof(Schedule.TimeRange), TimeRange.Create(startTime, endTime));
        SetPrivateProperty(schedule, nameof(Schedule.IsActive), true);
        return schedule;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (prop != null)
            {
                prop.SetValue(obj, value);
                return;
            }
            type = type.BaseType;
        }
    }
}

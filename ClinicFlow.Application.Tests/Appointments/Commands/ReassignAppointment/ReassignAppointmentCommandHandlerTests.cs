using ClinicFlow.Application.Appointments.Commands.ReassignAppointment;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ReassignAppointment;

public class ReassignAppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FakeTimeProvider _fakeTime;
    private readonly ReassignAppointmentCommandHandler _sut;

    public ReassignAppointmentCommandHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _doctorRepositoryMock = new Mock<IDoctorRepository>();
        _scheduleRepositoryMock = new Mock<IScheduleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fakeTime = new FakeTimeProvider();
        _sut = new ReassignAppointmentCommandHandler(
            _appointmentRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReassignAppointment_WhenValidCommand()
    {
        // Arrange
        var appointment = CreateDisplacedAppointment();
        var newDoctor = Doctor.Create(
            Guid.NewGuid(),
            MedicalLicenseNumber.Create("DOC123"),
            Guid.NewGuid(),
            "Specialist",
            ConsultationRoom.Create(1, "Room A", 1)
        );

        var newDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date;
        var command = new ReassignAppointmentCommand(
            appointment.Id,
            newDoctor.Id,
            newDate,
            TimeSpan.FromHours(10),
            TimeSpan.FromHours(11)
        );

        var shedule = Schedule.Create(
            newDoctor.Id,
            newDate.DayOfWeek,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(17))
        );

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(command.NewDoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newDoctor);

        _scheduleRepositoryMock
            .Setup(x =>
                x.GetByDoctorAndDayAsync(
                    newDoctor.Id,
                    newDate.DayOfWeek,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(shedule);

        _appointmentRepositoryMock
            .Setup(x =>
                x.HasConflictAsync(
                    newDoctor.Id,
                    newDate,
                    It.IsAny<TimeRange>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        appointment.DoctorId.Should().Be(newDoctor.Id);
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.DomainEvents.OfType<AppointmentReassignedEvent>().Should().ContainSingle();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new ReassignAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(10),
            TimeSpan.FromHours(11)
        );

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenNewDoctorNotFound()
    {
        // Arrange
        var appointment = CreateDisplacedAppointment();
        var command = new ReassignAppointmentCommand(
            appointment.Id,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            TimeSpan.FromHours(10),
            TimeSpan.FromHours(11)
        );

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _doctorRepositoryMock
            .Setup(x => x.GetByIdAsync(command.NewDoctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Doctor));
    }

    private Appointment CreateDisplacedAppointment()
    {
        var scheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date;
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDate,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10))
        );

        appointment.MarkAsRequiresReassignment();
        appointment.ClearDomainEvents();

        return appointment;
    }
}

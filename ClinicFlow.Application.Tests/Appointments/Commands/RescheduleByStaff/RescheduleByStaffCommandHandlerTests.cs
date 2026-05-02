using ClinicFlow.Application.Appointments.Commands.RescheduleByStaff;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByStaff;

public class RescheduleByStaffCommandHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IScheduleRepository> _scheduleRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RescheduleByStaffCommandHandler _sut;

    public RescheduleByStaffCommandHandlerTests()
    {
        _sut = new RescheduleByStaffCommandHandler(
            _appointmentRepositoryMock.Object,
            _scheduleRepositoryMock.Object,
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
        var appointment = CreateAppointment(Guid.NewGuid(), doctorId, Guid.NewGuid());
        var schedule = CreateSchedule(doctorId, newDate.DayOfWeek, newStartTime, newEndTime);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
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

    private Appointment CreateAppointment(Guid patientId, Guid doctorId, Guid typeId) =>
        Appointment.Schedule(
            patientId,
            doctorId,
            typeId,
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange.Create(new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0))
        );

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        TimeSpan start,
        TimeSpan end
    ) => Schedule.Create(doctorId, dayOfWeek, TimeRange.Create(start, end));
}

using ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByStaff;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.MarkAppointmentAsNoShowByStaff;

public class MarkAppointmentAsNoShowByStaffCommandHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MarkAppointmentAsNoShowByStaffCommandHandler _sut;

    public MarkAppointmentAsNoShowByStaffCommandHandlerTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new MarkAppointmentAsNoShowByStaffCommandHandler(
            _appointmentRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldMarkAsNoShow()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByStaffCommand(Guid.NewGuid());
        var appointment = CreateAppointment();

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        _appointmentRepositoryMock.Verify(
            x => x.UpdateAsync(appointment, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFound_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByStaffCommand(Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));
    }

    private Appointment CreateAppointment()
    {
        var scheduledDateTime = _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1);
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDateTime.Date,
            TimeRange.Create(
                scheduledDateTime.TimeOfDay,
                scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );
        return appointment;
    }
}

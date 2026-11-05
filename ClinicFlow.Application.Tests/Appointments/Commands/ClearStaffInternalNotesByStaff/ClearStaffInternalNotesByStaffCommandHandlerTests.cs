using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.ClearStaffInternalNotesByStaff;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ClearStaffInternalNotesByStaff;

public class ClearStaffInternalNotesByStaffCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly ClearStaffInternalNotesByStaffCommandHandler _sut;

    public ClearStaffInternalNotesByStaffCommandHandlerTests()
    {
        _sut = new ClearStaffInternalNotesByStaffCommandHandler(
            _appointmentRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldClearStaffInternalNotes_WhenAppointmentIsCheckedIn()
    {
        // Arrange
        var command = new ClearStaffInternalNotesByStaffCommand(Guid.CreateVersion7());

        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            Guid.CreateVersion7()
        );

        appointment.CheckIn(appointment.ScheduledDate.ToDateTime(TimeOnly.MinValue));
        appointment.UpdateStaffInternalNotes("notes");

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        appointment.StaffInternalNotes.Should().BeEmpty();

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new ClearStaffInternalNotesByStaffCommand(Guid.CreateVersion7());

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

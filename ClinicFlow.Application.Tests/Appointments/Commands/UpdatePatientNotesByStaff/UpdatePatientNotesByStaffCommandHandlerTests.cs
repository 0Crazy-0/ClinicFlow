using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.UpdatePatientNotesByStaff;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.UpdatePatientNotesByStaff;

public class UpdatePatientNotesByStaffCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly UpdatePatientNotesByStaffCommandHandler _sut;

    public UpdatePatientNotesByStaffCommandHandlerTests()
    {
        _sut = new UpdatePatientNotesByStaffCommandHandler(
            _appointmentRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdatePatientNotes_WhenValidRequest()
    {
        // Arrange
        var command = new UpdatePatientNotesByStaffCommand(Guid.NewGuid(), "Updated by staff");
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        appointment.PatientNotes.Should().Be(command.Notes);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new UpdatePatientNotesByStaffCommand(Guid.NewGuid(), "Notes");

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

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

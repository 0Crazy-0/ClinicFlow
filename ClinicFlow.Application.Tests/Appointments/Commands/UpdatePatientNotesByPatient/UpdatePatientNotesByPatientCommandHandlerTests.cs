using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.UpdatePatientNotesByPatient;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.UpdatePatientNotesByPatient;

public class UpdatePatientNotesByPatientCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly UpdatePatientNotesByPatientCommandHandler _sut;

    public UpdatePatientNotesByPatientCommandHandlerTests()
    {
        _sut = new UpdatePatientNotesByPatientCommandHandler(
            _appointmentRepositoryMock.Object,
            _familyMembershipRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdatePatientNotes_WhenInitiatorHasAccess()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var command = new UpdatePatientNotesByPatientCommand(
            Guid.CreateVersion7(),
            userId,
            "Updated patient notes"
        );

        var appointment = CreateAppointment(Guid.CreateVersion7());

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _familyMembershipRepositoryMock
            .Setup(r =>
                r.HasActiveMembershipAsync(
                    command.InitiatorUserId,
                    appointment.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        appointment.PatientNotes.Should().Be(command.Notes);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new UpdatePatientNotesByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Notes"
        );

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

    [Fact]
    public async Task Handle_ShouldThrowPatientAccessUnauthorizedException_WhenInitiatorHasNoAccess()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var command = new UpdatePatientNotesByPatientCommand(
            Guid.CreateVersion7(),
            initiatorUserId,
            "Notes"
        );

        var appointment = CreateAppointment(Guid.CreateVersion7());

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _familyMembershipRepositoryMock
            .Setup(r =>
                r.HasActiveMembershipAsync(
                    command.InitiatorUserId,
                    appointment.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Appointment CreateAppointment(Guid patientId) =>
        Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );
}

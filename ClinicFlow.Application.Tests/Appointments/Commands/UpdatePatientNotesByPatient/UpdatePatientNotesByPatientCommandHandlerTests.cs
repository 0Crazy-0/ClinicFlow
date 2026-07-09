using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.UpdatePatientNotesByPatient;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.UpdatePatientNotesByPatient;

public class UpdatePatientNotesByPatientCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly UpdatePatientNotesByPatientCommandHandler _sut;

    public UpdatePatientNotesByPatientCommandHandlerTests()
    {
        _sut = new UpdatePatientNotesByPatientCommandHandler(
            _appointmentRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdatePatientNotes_WhenInitiatorIsSelf()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var command = new UpdatePatientNotesByPatientCommand(
            Guid.CreateVersion7(),
            userId,
            "Updated patient notes"
        );

        var targetPatient = CreatePatientSelf(userId);
        var appointment = CreateAppointment(targetPatient.Id);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);

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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        // Arrange
        var command = new UpdatePatientNotesByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Notes"
        );
        var appointment = CreateAppointment(Guid.CreateVersion7());

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePatientSelf(command.InitiatorUserId));

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenInitiatorPatientNotFound()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var command = new UpdatePatientNotesByPatientCommand(
            Guid.CreateVersion7(),
            userId,
            "Notes"
        );
        var targetPatient = CreatePatientSelf(userId);
        var appointment = CreateAppointment(targetPatient.Id);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetPatient);
        _patientRepositoryMock
            .Setup(r => r.GetByUserIdAsync(command.InitiatorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Patient CreatePatientSelf(Guid userId) =>
        Patient.CreateSelf(
            userId,
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private Appointment CreateAppointment(Guid patientId) =>
        Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0))
        );
}

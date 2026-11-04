using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.ClearPatientNotesByPatient;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ClearPatientNotesByPatient;

public class ClearPatientNotesByPatientCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly ClearPatientNotesByPatientCommandHandler _sut;

    public ClearPatientNotesByPatientCommandHandlerTests()
    {
        _sut = new ClearPatientNotesByPatientCommandHandler(
            _appointmentRepositoryMock.Object,
            _familyMembershipRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldClearPatientNotes_WhenInitiatorIsSelf()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var command = new ClearPatientNotesByPatientCommand(Guid.CreateVersion7(), userId);

        var patientId = Guid.CreateVersion7();
        var appointment = CreateAppointment(patientId);

        appointment.UpdatePatientNotes("notes");

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _familyMembershipRepositoryMock
            .Setup(r =>
                r.GetActiveMembershipAsync(
                    command.InitiatorUserId,
                    appointment.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                FamilyMembership.CreateSelf(patientId, userId, _fakeTime.GetUtcNow().UtcDateTime)
            );

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        appointment.PatientNotes.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new ClearPatientNotesByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
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

        _familyMembershipRepositoryMock.Verify(
            r =>
                r.GetActiveMembershipAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowPatientAccessUnauthorizedException_WhenNoMembership()
    {
        // Arrange
        var command = new ClearPatientNotesByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        var appointment = CreateAppointment(Guid.CreateVersion7());

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _familyMembershipRepositoryMock
            .Setup(r =>
                r.GetActiveMembershipAsync(
                    command.InitiatorUserId,
                    appointment.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FamilyMembership?)null);

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowPatientAccessUnauthorizedException_WhenInitiatorIsGuardian()
    {
        // Arrange
        var guardianId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var command = new ClearPatientNotesByPatientCommand(Guid.CreateVersion7(), guardianId);

        var appointment = CreateAppointment(patientId);
        appointment.UpdatePatientNotes("notes");

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _familyMembershipRepositoryMock
            .Setup(r =>
                r.GetActiveMembershipAsync(
                    command.InitiatorUserId,
                    appointment.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                FamilyMembership.CreateFamilyMember(
                    patientId,
                    guardianId,
                    PatientRelationship.Parent,
                    LegalAuthorityType.Parent,
                    FamilyMembershipAccessLevel.Full,
                    10,
                    _fakeTime.GetUtcNow().UtcDateTime
                )
            );

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);

        appointment.PatientNotes.Should().Be("notes");

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Appointment CreateAppointment(Guid patientId) =>
        Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            Guid.CreateVersion7()
        );
}

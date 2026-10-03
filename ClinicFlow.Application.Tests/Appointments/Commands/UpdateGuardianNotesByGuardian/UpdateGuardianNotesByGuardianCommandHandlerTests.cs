using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.UpdateGuardianNotesByGuardian;
using ClinicFlow.Application.Tests.Shared;
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

namespace ClinicFlow.Application.Tests.Appointments.Commands.UpdateGuardianNotesByGuardian;

public class UpdateGuardianNotesByGuardianCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly UpdateGuardianNotesByGuardianCommandHandler _sut;

    public UpdateGuardianNotesByGuardianCommandHandlerTests()
    {
        _sut = new UpdateGuardianNotesByGuardianCommandHandler(
            _appointmentRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _familyMembershipRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateGuardianNotes_WhenAuthorGuardianWithFullAndAuthority()
    {
        // Arrange
        var authorId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var command = new UpdateGuardianNotesByGuardianCommand(
            Guid.CreateVersion7(),
            authorId,
            "Updated note"
        );

        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            null,
            Guid.CreateVersion7()
        );

        appointment.SetGuardianNotes("Original", authorId);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _familyMembershipRepositoryMock
            .Setup(r =>
                r.GetActiveMembershipAsync(
                    authorId,
                    appointment.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                FamilyMembership.CreateFamilyMember(
                    appointment.PatientId,
                    authorId,
                    PatientRelationship.Parent,
                    LegalAuthorityType.Parent,
                    FamilyMembershipAccessLevel.Full,
                    10,
                    _fakeTime.GetUtcNow().UtcDateTime
                )
            );

        var patient = Patient.CreateProfile(
            PersonName.Create("Minor"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.SetId(appointment.PatientId);

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        appointment.GuardianNotes.Should().Be(command.Notes);
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
        appointment.PatientNotes.Should().BeEmpty();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new UpdateGuardianNotesByGuardianCommand(
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
    public async Task Handle_ShouldThrowPatientAccessUnauthorizedException_WhenNoMembership()
    {
        // Arrange
        var authorId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var command = new UpdateGuardianNotesByGuardianCommand(
            Guid.CreateVersion7(),
            authorId,
            "Notes"
        );

        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            null,
            Guid.CreateVersion7()
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _familyMembershipRepositoryMock
            .Setup(r =>
                r.GetActiveMembershipAsync(
                    authorId,
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
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        // Arrange
        var authorId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var command = new UpdateGuardianNotesByGuardianCommand(
            Guid.CreateVersion7(),
            authorId,
            "Notes"
        );

        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            null,
            Guid.CreateVersion7()
        );

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _familyMembershipRepositoryMock
            .Setup(r =>
                r.GetActiveMembershipAsync(
                    authorId,
                    appointment.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                FamilyMembership.CreateFamilyMember(
                    appointment.PatientId,
                    authorId,
                    PatientRelationship.Parent,
                    LegalAuthorityType.Parent,
                    FamilyMembershipAccessLevel.Full,
                    10,
                    _fakeTime.GetUtcNow().UtcDateTime
                )
            );
        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
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
}

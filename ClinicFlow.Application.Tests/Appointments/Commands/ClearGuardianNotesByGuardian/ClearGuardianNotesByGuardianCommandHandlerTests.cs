using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Commands.ClearGuardianNotesByGuardian;
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

namespace ClinicFlow.Application.Tests.Appointments.Commands.ClearGuardianNotesByGuardian;

public class ClearGuardianNotesByGuardianCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly ClearGuardianNotesByGuardianCommandHandler _sut;

    public ClearGuardianNotesByGuardianCommandHandlerTests()
    {
        _sut = new ClearGuardianNotesByGuardianCommandHandler(
            _appointmentRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _familyMembershipRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldClearGuardianNotesAndAuthor_WhenOriginalAuthorClears()
    {
        // Arrange
        var authorId = Guid.CreateVersion7();

        var command = new ClearGuardianNotesByGuardianCommand(Guid.CreateVersion7(), authorId);

        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            Guid.CreateVersion7()
        );

        appointment.UpdateGuardianNotes("notes", authorId);

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
            .ReturnsAsync(CreateMinorPatient(appointment.PatientId));

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        appointment.GuardianNotes.Should().BeEmpty();
        appointment.GuardianNotesAuthorUserId.Should().BeNull();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenAppointmentNotFound()
    {
        // Arrange
        var command = new ClearGuardianNotesByGuardianCommand(
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
        _patientRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowPatientAccessUnauthorizedException_WhenNoMembership()
    {
        // Arrange
        var command = new ClearGuardianNotesByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            Guid.CreateVersion7()
        );

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

        _familyMembershipRepositoryMock.Verify(
            r =>
                r.GetActiveMembershipAsync(
                    command.InitiatorUserId,
                    appointment.PatientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowPatientAccessUnauthorizedException_WhenRequesterIsNotNotesAuthor()
    {
        // Arrange
        var authorId = Guid.CreateVersion7();
        var requesterId = Guid.CreateVersion7();

        var command = new ClearGuardianNotesByGuardianCommand(Guid.CreateVersion7(), requesterId);

        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
            Guid.CreateVersion7()
        );

        appointment.UpdateGuardianNotes("notes", authorId);

        _appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(command.AppointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _familyMembershipRepositoryMock
            .Setup(r =>
                r.GetActiveMembershipAsync(
                    requesterId,
                    appointment.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                FamilyMembership.CreateFamilyMember(
                    appointment.PatientId,
                    requesterId,
                    PatientRelationship.Parent,
                    LegalAuthorityType.Parent,
                    FamilyMembershipAccessLevel.Full,
                    10,
                    _fakeTime.GetUtcNow().UtcDateTime
                )
            );

        _patientRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMinorPatient(appointment.PatientId));

        // Act
        var act = async () => await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);

        appointment.GuardianNotes.Should().Be("notes");
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);

        _familyMembershipRepositoryMock.Verify(
            r =>
                r.GetActiveMembershipAsync(
                    requesterId,
                    appointment.PatientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        // Arrange
        var authorId = Guid.CreateVersion7();
        var command = new ClearGuardianNotesByGuardianCommand(Guid.CreateVersion7(), authorId);

        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(10), new TimeOnly(11)),
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

    private Patient CreateMinorPatient(Guid patientId)
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Minor", null, "Minor"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.SetId(patientId);

        return patient;
    }
}

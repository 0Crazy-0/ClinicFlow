using AwesomeAssertions;
using ClinicFlow.Application.FamilyMemberships.Commands.ChangeAccessLevel;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.FamilyMemberships.Commands.ChangeAccessLevel;

public class ChangeAccessLevelCommandHandlerTests
{
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ChangeAccessLevelCommandHandler _sut;

    public ChangeAccessLevelCommandHandlerTests()
    {
        _sut = new ChangeAccessLevelCommandHandler(
            _familyMembershipRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _fakeTime,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldChangeAccessLevelAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var membership = CreateFamilyMembership(patient.Id, targetUserId);

        var command = new ChangeAccessLevelCommand(
            requesterUserId,
            targetUserId,
            patient.Id,
            FamilyMembershipAccessLevel.Restricted
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(targetUserId, patient.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipByUserIdAsync(
                    requesterUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        var requesterMembership = CreateFamilyMembership(patient.Id, requesterUserId);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(requesterMembership);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Restricted);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldChangeAccessLevelAndSaveChanges_WhenRequesterIsPatientsSelf()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        var patient = Patient.CreateProfile(
            PersonName.Create("Jane Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-25)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var membership = CreateFamilyMembership(patient.Id, targetUserId);

        var command = new ChangeAccessLevelCommand(
            requesterUserId,
            targetUserId,
            patient.Id,
            FamilyMembershipAccessLevel.Restricted
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(targetUserId, patient.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipByUserIdAsync(
                    requesterUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        var requesterMembership = FamilyMembership.CreateSelf(
            patient.Id,
            requesterUserId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(requesterMembership);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Restricted);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenRequesterHasNoActiveMembershipWithPatient()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var membership = CreateFamilyMembership(patient.Id, targetUserId);

        var command = new ChangeAccessLevelCommand(
            requesterUserId,
            targetUserId,
            patient.Id,
            FamilyMembershipAccessLevel.Restricted
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(targetUserId, patient.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipByUserIdAsync(
                    requesterUserId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patient.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FamilyMembership?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.UnauthorizedAccessLevelChange);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenTargetMembershipDoesNotExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();

        var command = new ChangeAccessLevelCommand(
            requesterUserId,
            targetUserId,
            patientId,
            FamilyMembershipAccessLevel.Restricted
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(targetUserId, patientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((FamilyMembership?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(FamilyMembership));

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientDoesNotExist()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var targetUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var membership = CreateFamilyMembership(patientId, targetUserId);

        var command = new ChangeAccessLevelCommand(
            requesterUserId,
            targetUserId,
            patientId,
            FamilyMembershipAccessLevel.Restricted
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(targetUserId, patientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private FamilyMembership CreateFamilyMembership(Guid patientId, Guid ownerUserId) =>
        FamilyMembership.CreateFamilyMember(
            patientId,
            ownerUserId,
            PatientRelationship.Child,
            FamilyMembershipAccessLevel.Full,
            _fakeTime.GetUtcNow().UtcDateTime
        );
}

using AwesomeAssertions;
using ClinicFlow.Application.FamilyMemberships.Commands.LeaveFamilyMembership;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.FamilyMemberships.Commands.LeaveFamilyMembership;

public class LeaveFamilyMembershipCommandHandlerTests
{
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly LeaveFamilyMembershipCommandHandler _sut;

    public LeaveFamilyMembershipCommandHandlerTests()
    {
        _sut = new LeaveFamilyMembershipCommandHandler(
            _familyMembershipRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _fakeTime,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldLeaveMembershipAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var startedAt = _fakeTime.GetUtcNow().UtcDateTime;
        var membership = CreateFamilyMembership(patientId, userId, startedAt);

        _fakeTime.Advance(TimeSpan.FromSeconds(1));
        var leaveTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = Patient.CreateProfile(
            PersonName.Create("Adult Member"),
            dateOfBirth: DateOnly.FromDateTime(leaveTime.AddYears(-20)),
            startedAt
        );

        var command = new LeaveFamilyMembershipCommand(userId, patientId);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(userId, patientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        membership.Status.Should().Be(FamilyMembershipStatus.Left);
        membership.EndedAt.Should().Be(leaveTime);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMembershipDoesNotExist()
    {
        // Arrange
        var command = new LeaveFamilyMembershipCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    command.UserId,
                    command.PatientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FamilyMembership?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(FamilyMembership));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientDoesNotExist()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var membership = CreateFamilyMembership(
            patientId,
            userId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var command = new LeaveFamilyMembershipCommand(userId, patientId);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(userId, patientId, It.IsAny<CancellationToken>())
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

    private static FamilyMembership CreateFamilyMembership(
        Guid patientId,
        Guid ownerUserId,
        DateTime startedAt
    ) =>
        FamilyMembership.CreateFamilyMember(
            patientId,
            ownerUserId,
            PatientRelationship.Child,
            FamilyMembershipAccessLevel.Full,
            startedAt
        );
}

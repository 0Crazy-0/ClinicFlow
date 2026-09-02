using AwesomeAssertions;
using ClinicFlow.Application.FamilyMemberships.Commands.RevokeFamilyMember;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.FamilyMemberships.Commands.RevokeFamilyMember;

public class RevokeFamilyMemberCommandHandlerTests
{
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RevokeFamilyMemberCommandHandler _sut;

    public RevokeFamilyMemberCommandHandlerTests()
    {
        _unitOfWorkMock
            .Setup(x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(
                (
                    Guid _,
                    Func<CancellationToken, Task> operation,
                    CancellationToken cancellationToken
                ) => operation(cancellationToken)
            );

        _sut = new RevokeFamilyMemberCommandHandler(
            _familyMembershipRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _fakeTime,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldRevokeMembershipAndSaveChanges_WhenValidCommand()
    {
        // Arrange
        var ownerUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var startedAt = _fakeTime.GetUtcNow().UtcDateTime;
        var membership = CreateFamilyMembership(patientId, ownerUserId, startedAt);

        _fakeTime.Advance(TimeSpan.FromDays(1));
        var revokeTime = _fakeTime.GetUtcNow().UtcDateTime;

        var command = new RevokeFamilyMemberCommand(ownerUserId, patientId);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(ownerUserId, patientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(membership);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipByPatientIdAsync(patientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);

        _appointmentRepositoryMock
            .Setup(x =>
                x.HasUpcomingAppointmentRequiringGuardianForMinorAsync(
                    patientId,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    command.PatientId,
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        membership.Status.Should().Be(FamilyMembershipStatus.Revoked);
        membership.EndedAt.Should().Be(revokeTime);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMembershipDoesNotExist()
    {
        // Arrange
        var command = new RevokeFamilyMemberCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    command.OwnerUserId,
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

        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenPatientHasUpcomingAppointmentRequiringGuardianForMinor()
    {
        // Arrange
        var ownerUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var startedAt = _fakeTime.GetUtcNow().UtcDateTime;
        var membership = CreateFamilyMembership(patientId, ownerUserId, startedAt);

        _fakeTime.Advance(TimeSpan.FromDays(1));

        var command = new RevokeFamilyMemberCommand(ownerUserId, patientId);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(ownerUserId, patientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(membership);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipByPatientIdAsync(patientId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(true);

        _appointmentRepositoryMock
            .Setup(x =>
                x.HasUpcomingAppointmentRequiringGuardianForMinorAsync(
                    patientId,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CannotRemoveWithUpcomingAppointments);

        _unitOfWorkMock.Verify(
            x =>
                x.ExecuteWithLockAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
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

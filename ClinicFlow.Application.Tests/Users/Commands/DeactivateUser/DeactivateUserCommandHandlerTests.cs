using AwesomeAssertions;
using ClinicFlow.Application.Users.Commands.DeactivateUser;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Commands.DeactivateUser;

public class DeactivateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DeactivateUserCommandHandler _sut;

    public DeactivateUserCommandHandlerTests()
    {
        _sut = new DeactivateUserCommandHandler(
            _userRepositoryMock.Object,
            _familyMembershipRepositoryMock.Object,
            _fakeTime,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldDeactivateUserAndLeaveSelfMembership_WhenValidRequest()
    {
        // Arrange
        var user = CreateUser();
        var startTime = _fakeTime.GetUtcNow().UtcDateTime;
        var selfMembership = CreateSelfMembership(user.Id, startTime);

        _fakeTime.Advance(TimeSpan.FromHours(1));
        var command = new DeactivateUserCommand(user.Id);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveSelfMembershipByUserIdAsync(user.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(selfMembership);

        _familyMembershipRepositoryMock
            .Setup(x => x.CountActiveFamilyMembersAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        user.IsActive.Should().BeFalse();
        selfMembership.Status.Should().Be(FamilyMembershipStatus.Left);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new DeactivateUserCommand(Guid.CreateVersion7());

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(User));

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveSelfMembershipByUserIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFound_WhenActiveSelfMembershipDoesNotExist()
    {
        // Arrange
        var user = CreateUser();
        var command = new DeactivateUserCommand(user.Id);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveSelfMembershipByUserIdAsync(user.Id, It.IsAny<CancellationToken>())
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
            x => x.CountActiveFamilyMembersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenUserHasActiveFamilyMembers()
    {
        // Arrange
        var user = CreateUser();
        var selfMembership = CreateSelfMembership(user.Id, _fakeTime.GetUtcNow().UtcDateTime);
        var command = new DeactivateUserCommand(user.Id);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveSelfMembershipByUserIdAsync(user.Id, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(selfMembership);

        _familyMembershipRepositoryMock
            .Setup(x => x.CountActiveFamilyMembersAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var act = () => _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.User.CannotCloseAccountWithActiveFamilyMembers);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static User CreateUser() =>
        User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );

    private static FamilyMembership CreateSelfMembership(Guid userId, DateTime referenceTime) =>
        FamilyMembership.CreateSelf(Guid.CreateVersion7(), userId, referenceTime);
}

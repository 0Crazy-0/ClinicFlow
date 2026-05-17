using ClinicFlow.Application.Users.Commands.DeactivateUser;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Commands.DeactivateUser;

public class DeactivateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DeactivateUserCommandHandler _sut;

    public DeactivateUserCommandHandlerTests()
    {
        _sut = new DeactivateUserCommandHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeactivateUser_WhenUserIsActive()
    {
        // Arrange
        var user = CreateUser();
        var command = new DeactivateUserCommand(user.Id);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new DeactivateUserCommand(Guid.NewGuid());

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(User));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserIsAlreadyInactive()
    {
        // Arrange
        var user = CreateUser();
        var command = new DeactivateUserCommand(user.Id);

        user.Deactivate();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.AlreadyInactive);
    }

    private static User CreateUser() =>
        User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );
}

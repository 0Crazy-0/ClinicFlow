using ClinicFlow.Application.Users.Commands.ChangePassword;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasherService> _passwordHasherServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ChangePasswordCommandHandler _sut;

    public ChangePasswordCommandHandlerTests()
    {
        _sut = new ChangePasswordCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldChangePassword_WhenCurrentPasswordIsValid()
    {
        // Arrange
        var user = CreateUser();
        var command = new ChangePasswordCommand(user.Id, "currentpassword", "newpassword123");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherServiceMock
            .Setup(x => x.Verify(command.CurrentPassword, user.PasswordHash))
            .Returns(true);

        _passwordHasherServiceMock
            .Setup(x => x.Hash(command.NewPassword))
            .Returns("new_hashed_password");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        user.PasswordHash.Should().Be("new_hashed_password");
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new ChangePasswordCommand(
            Guid.NewGuid(),
            "currentpassword",
            "newpassword123"
        );

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
    public async Task Handle_ShouldThrowInvalidCredentials_WhenCurrentPasswordIsWrong()
    {
        // Arrange
        var user = CreateUser();
        var command = new ChangePasswordCommand(user.Id, "currentpassword", "newpassword123");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherServiceMock
            .Setup(x => x.Verify(command.CurrentPassword, user.PasswordHash))
            .Returns(false);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.InvalidCredentials);
    }

    private static User CreateUser() =>
        User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );
}

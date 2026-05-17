using ClinicFlow.Application.Users.Commands.LoginUser;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Commands.LoginUser;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasherService> _passwordHasherServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly LoginUserCommandHandler _sut;

    public LoginUserCommandHandlerTests()
    {
        _sut = new LoginUserCommandHandler(
            _fakeTime,
            _userRepositoryMock.Object,
            _passwordHasherServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnUserId_WhenCredentialsAreValid()
    {
        // Arrange
        var user = CreateUser();
        var comamand = new LoginUserCommand(user.Email.Value, "password123");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(comamand.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherServiceMock
            .Setup(x => x.Verify(comamand.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _sut.Handle(comamand, CancellationToken.None);

        // Assert
        result.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_WhenLoginIsSuccessful()
    {
        // Arrange
        var user = CreateUser();
        var comamand = new LoginUserCommand(user.Email.Value, "password123");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(comamand.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherServiceMock
            .Setup(x => x.Verify(comamand.Password, user.PasswordHash))
            .Returns(true);

        // Act
        await _sut.Handle(comamand, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCredentials_WhenUserNotFound()
    {
        // Arrange
        var comamand = new LoginUserCommand("nonexistent@clinic.com", "password123");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(comamand.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.Handle(comamand, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCredentialsAndPersist_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = CreateUser();
        var command = new LoginUserCommand(user.Email.Value, "wrongpassword");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherServiceMock
            .Setup(x => x.Verify(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.InvalidCredentials);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static User CreateUser() =>
        User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );
}

using AwesomeAssertions;
using ClinicFlow.Application.Interfaces;
using ClinicFlow.Application.Users.Commands.ResetPassword;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IPasswordResetTokenService> _passwordResetTokenServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasherService> _passwordHasherServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ResetPasswordCommandHandler _sut;

    public ResetPasswordCommandHandlerTests()
    {
        _sut = new ResetPasswordCommandHandler(
            _passwordResetTokenServiceMock.Object,
            _userRepositoryMock.Object,
            _passwordHasherServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldChangePassword_WhenTokenIsValid()
    {
        // Arrange
        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );

        var command = new ResetPasswordCommand("valid-token", "newpassword123");

        _passwordResetTokenServiceMock
            .Setup(x => x.ValidateTokenAsync(command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.Id);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

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
    public async Task Handle_ShouldThrowException_WhenTokenIsInvalid()
    {
        // Arrange
        var command = new ResetPasswordCommand("invalid-token", "newpassword123");

        _passwordResetTokenServiceMock
            .Setup(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new ResetPasswordCommand("valid-token", "newpassword123");

        _passwordResetTokenServiceMock
            .Setup(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
}

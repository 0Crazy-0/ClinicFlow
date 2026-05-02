using ClinicFlow.Application.Users.Commands.VerifyPhone;
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

namespace ClinicFlow.Application.Tests.Users.Commands.VerifyPhone;

public class VerifyPhoneCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPhoneVerificationService> _phoneVerificationServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly VerifyPhoneCommandHandler _sut;

    public VerifyPhoneCommandHandlerTests()
    {
        _sut = new VerifyPhoneCommandHandler(
            _userRepositoryMock.Object,
            _phoneVerificationServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldMarkPhoneAsVerified_WhenCodeIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new VerifyPhoneCommand(userId, "123456");
        var phone = PhoneNumber.Create("555-1234");

        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword",
            phone,
            UserRole.Patient
        );

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _phoneVerificationServiceMock
            .Setup(x => x.VerifyCodeAsync(phone, "123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        user.IsPhoneVerified.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCodeIsInvalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new VerifyPhoneCommand(userId, "wrong-code");
        var phone = PhoneNumber.Create("555-1234");

        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword",
            phone,
            UserRole.Patient
        );

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _phoneVerificationServiceMock
            .Setup(x => x.VerifyCodeAsync(phone, "wrong-code", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.User.InvalidVerificationCode);

        user.IsPhoneVerified.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var command = new VerifyPhoneCommand(Guid.NewGuid(), "123456");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(User));

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

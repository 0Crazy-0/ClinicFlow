using ClinicFlow.Application.Users.Commands.SendPhoneVerification;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Interfaces.Services;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Commands.SendPhoneVerification;

public class SendPhoneVerificationCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPhoneVerificationService> _phoneVerificationServiceMock = new();
    private readonly SendPhoneVerificationCommandHandler _sut;

    public SendPhoneVerificationCommandHandlerTests()
    {
        _sut = new SendPhoneVerificationCommandHandler(
            _userRepositoryMock.Object,
            _phoneVerificationServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldSendVerificationCode_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SendPhoneVerificationCommand(userId);
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

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _phoneVerificationServiceMock.Verify(
            x => x.SendVerificationCodeAsync(phone, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var command = new SendPhoneVerificationCommand(Guid.NewGuid());

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
    }
}

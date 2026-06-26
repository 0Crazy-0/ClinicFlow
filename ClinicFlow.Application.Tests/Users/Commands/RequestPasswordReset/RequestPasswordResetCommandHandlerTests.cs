using ClinicFlow.Application.Interfaces;
using ClinicFlow.Application.Users.Commands.RequestPasswordReset;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Commands.RequestPasswordReset;

public class RequestPasswordResetCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordResetTokenService> _tokenServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly RequestPasswordResetCommandHandler _sut;

    private static readonly RequestPasswordResetCommand ValidCommand = new("test@clinic.com");

    public RequestPasswordResetCommandHandlerTests()
    {
        _sut = new RequestPasswordResetCommandHandler(
            _userRepositoryMock.Object,
            _tokenServiceMock.Object,
            _emailServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldGenerateTokenAndSendEmail_WhenUserExists()
    {
        // Arrange
        var user = User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(x => x.GenerateTokenAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync("reset-token");

        // Act
        await _sut.Handle(ValidCommand, TestContext.Current.CancellationToken);

        // Assert
        _tokenServiceMock.Verify(
            x => x.GenerateTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _emailServiceMock.Verify(
            x =>
                x.SendPasswordResetEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldSilentlyReturn_WhenUserNotFound()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _sut.Handle(ValidCommand, TestContext.Current.CancellationToken);

        // Assert
        _tokenServiceMock.Verify(
            x => x.GenerateTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _emailServiceMock.Verify(
            x =>
                x.SendPasswordResetEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}

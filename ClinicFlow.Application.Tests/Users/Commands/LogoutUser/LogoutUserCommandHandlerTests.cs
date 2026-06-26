using ClinicFlow.Application.Interfaces;
using ClinicFlow.Application.Users.Commands.LogoutUser;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Commands.LogoutUser;

public class LogoutUserCommandHandlerTests
{
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly LogoutUserCommandHandler _sut;

    public LogoutUserCommandHandlerTests()
    {
        _sut = new LogoutUserCommandHandler(_refreshTokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallRevokeAsync_WhenValidCommand()
    {
        // Arrange
        var command = new LogoutUserCommand(Guid.NewGuid());

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _refreshTokenServiceMock.Verify(
            x => x.RevokeAsync(command.UserId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}

using AwesomeAssertions;
using ClinicFlow.Application.Users.Queries.CheckPhoneUniqueness;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Queries.CheckPhoneUniqueness;

public class CheckPhoneUniquenessQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly CheckPhoneUniquenessQueryHandler _sut;

    public CheckPhoneUniquenessQueryHandlerTests()
    {
        _sut = new CheckPhoneUniquenessQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenPhoneNumberIsUnique()
    {
        // Arrange
        var query = new CheckPhoneUniquenessQuery("555-9999");

        _userRepositoryMock
            .Setup(x => x.ExistsByPhoneNumberAsync("555-9999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();

        _userRepositoryMock.Verify(
            x => x.ExistsByPhoneNumberAsync("555-9999", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenPhoneNumberAlreadyExists()
    {
        // Arrange
        var query = new CheckPhoneUniquenessQuery("555-0000");

        _userRepositoryMock
            .Setup(x => x.ExistsByPhoneNumberAsync("555-0000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();

        _userRepositoryMock.Verify(
            x => x.ExistsByPhoneNumberAsync("555-0000", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}

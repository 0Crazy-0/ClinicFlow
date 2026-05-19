using ClinicFlow.Application.Users.Queries.CheckEmailUniqueness;
using ClinicFlow.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ClinicFlow.Application.Tests.Users.Queries.CheckEmailUniqueness;

public class CheckEmailUniquenessQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly CheckEmailUniquenessQueryHandler _sut;

    public CheckEmailUniquenessQueryHandlerTests()
    {
        _sut = new CheckEmailUniquenessQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenEmailIsUnique()
    {
        // Arrange
        var query = new CheckEmailUniquenessQuery("unique@clinic.com");

        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync("unique@clinic.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenEmailAlreadyExists()
    {
        // Arrange
        var query = new CheckEmailUniquenessQuery("taken@clinic.com");

        _userRepositoryMock
            .Setup(x => x.ExistsByEmailAsync("taken@clinic.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}

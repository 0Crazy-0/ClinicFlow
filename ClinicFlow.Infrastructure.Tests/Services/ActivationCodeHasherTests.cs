using AwesomeAssertions;
using ClinicFlow.Infrastructure.Services;

namespace ClinicFlow.Infrastructure.Tests.Services;

public class ActivationCodeHasherTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowInvalidOperationException_WhenKeyIsMissing(string? key)
    {
        // Arrange && Act
        var act = () => new ActivationCodeHasher(key!);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(ActivationCodeHasher.MissingKeyMessage);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidOperationException_WhenKeyIsNotBase64()
    {
        // Arrange && Act
        var act = () => new ActivationCodeHasher("not base64!!");

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(ActivationCodeHasher.InvalidKeyMessage);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidOperationException_WhenKeyIsTooShort()
    {
        // Arrange
        var key = Convert.ToBase64String(Enumerable.Repeat((byte)0xAB, 16).ToArray());

        // Act
        var act = () => new ActivationCodeHasher(key);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(ActivationCodeHasher.TooShortKeyMessage);
    }

    [Fact]
    public void Constructor_ShouldNotIncludeKeyValueInMessage_WhenKeyIsInvalid()
    {
        // Arrange
        var key = "not base64!!";

        // Act
        var act = () => new ActivationCodeHasher(key);

        // Assert
        act.Should().Throw<InvalidOperationException>().Which.Message.Should().NotContain(key);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("1234567a")]
    [InlineData("abcdefgh")]
    public void ComputeHash_ShouldThrowArgumentException_WhenActivationCodeIsInvalid(
        string? activationCode
    )
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.ComputeHash(Guid.CreateVersion7(), activationCode!);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName(nameof(activationCode));
    }

    [Fact]
    public void ComputeHash_ShouldReturnLowercaseHexDigest_WhenActivationCodeIsValid()
    {
        // Arrange
        var sut = CreateSut();
        var userId = Guid.CreateVersion7();

        // Act
        var result = sut.ComputeHash(userId, "12345678");

        // Assert
        result.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void ComputeHash_ShouldReturnSameDigest_WhenCalledTwiceWithSameInputs()
    {
        // Arrange
        var sut = CreateSut();
        var userId = Guid.CreateVersion7();

        // Act
        var firstHash = sut.ComputeHash(userId, "12345678");
        var secondHash = sut.ComputeHash(userId, "12345678");

        // Assert
        secondHash.Should().Be(firstHash);
    }

    [Fact]
    public void ComputeHash_ShouldReturnDifferentDigests_WhenInputsDiffer()
    {
        // Arrange
        var sut = CreateSut();
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();

        // Act
        var baseHash = sut.ComputeHash(userId, "12345678");
        var otherCodeHash = sut.ComputeHash(userId, "87654321");
        var otherUserHash = sut.ComputeHash(otherUserId, "12345678");

        // Assert
        otherCodeHash.Should().NotBe(baseHash);
        otherUserHash.Should().NotBe(baseHash);
    }

    private static ActivationCodeHasher CreateSut() =>
        new(Convert.ToBase64String(Enumerable.Repeat((byte)0xAB, 32).ToArray()));
}

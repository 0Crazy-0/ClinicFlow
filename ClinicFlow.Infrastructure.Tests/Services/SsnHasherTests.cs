using AwesomeAssertions;
using ClinicFlow.Infrastructure.Services;

namespace ClinicFlow.Infrastructure.Tests.Services;

public class SsnHasherTests
{
    [Theory]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("abcd")]
    [InlineData("673a")]
    public void ComputeHash_ShouldThrowArgumentException_WhenLast4IsInvalid(string last4)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.ComputeHash(Guid.CreateVersion7(), last4);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName(nameof(last4));
    }

    [Fact]
    public void ComputeHash_ShouldReturnLowercaseHexDigest_WhenLast4IsValid()
    {
        // Arrange
        var sut = CreateSut();
        var patientId = Guid.CreateVersion7();

        // Act
        var result = sut.ComputeHash(patientId, "1234");

        // Assert
        result.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void ComputeHash_ShouldReturnSameDigest_WhenCalledTwiceWithSameInputs()
    {
        // Arrange
        var sut = CreateSut();
        var patientId = Guid.CreateVersion7();

        // Act
        var firstHash = sut.ComputeHash(patientId, "1234");
        var secondHash = sut.ComputeHash(patientId, "1234");

        // Assert
        secondHash.Should().Be(firstHash);
    }

    [Fact]
    public void ComputeHash_ShouldReturnDifferentDigests_WhenInputsDiffer()
    {
        // Arrange
        var sut = CreateSut();
        var patientId = Guid.CreateVersion7();
        var otherPatientId = Guid.CreateVersion7();

        // Act
        var baseHash = sut.ComputeHash(patientId, "1234");
        var otherLast4Hash = sut.ComputeHash(patientId, "5678");
        var otherPatientHash = sut.ComputeHash(otherPatientId, "1234");

        // Assert
        otherLast4Hash.Should().NotBe(baseHash);
        otherPatientHash.Should().NotBe(baseHash);
    }

    private static SsnHasher CreateSut() =>
        new(Convert.ToBase64String(Enumerable.Repeat((byte)0xAB, 32).ToArray()));
}

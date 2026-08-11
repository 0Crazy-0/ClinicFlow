using AwesomeAssertions;
using ClinicFlow.Application.Common.Utilities;

namespace ClinicFlow.Application.Tests.Common.Utilities;

public class DeterministicKeyGeneratorTests
{
    [Fact]
    public void FromComposite_ShouldReturnSameGuid_ForSameInputs()
    {
        // Arrange
        var first = "JOHN DOE";
        var second = "1990-01-01";

        // Act
        var key1 = DeterministicKeyGenerator.FromComposite(first, second);
        var key2 = DeterministicKeyGenerator.FromComposite(first, second);

        // Assert
        key1.Should().Be(key2);
    }

    [Fact]
    public void FromComposite_ShouldReturnDifferentGuid_ForDifferentFirstInputs()
    {
        // Arrange
        var first1 = "JOHN DOE";
        var first2 = "JANE DOE";
        var second = "1990-01-01";

        // Act
        var key1 = DeterministicKeyGenerator.FromComposite(first1, second);
        var key2 = DeterministicKeyGenerator.FromComposite(first2, second);

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void FromComposite_ShouldReturnDifferentGuid_ForDifferentSecondInputs()
    {
        // Arrange
        var first = "JOHN DOE";
        var second1 = "1990-01-01";
        var second2 = "1990-01-02";

        // Act
        var key1 = DeterministicKeyGenerator.FromComposite(first, second1);
        var key2 = DeterministicKeyGenerator.FromComposite(first, second2);

        // Assert
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void FromComposite_ShouldReturnDifferentGuid_ForInputsWithOverlappingBoundaries()
    {
        // Arrange
        var first1 = "AB";
        var second1 = "C";

        var first2 = "A";
        var second2 = "BC";

        // Act
        var key1 = DeterministicKeyGenerator.FromComposite(first1, second1);
        var key2 = DeterministicKeyGenerator.FromComposite(first2, second2);

        // Assert
        key1.Should().NotBe(key2);
    }
}

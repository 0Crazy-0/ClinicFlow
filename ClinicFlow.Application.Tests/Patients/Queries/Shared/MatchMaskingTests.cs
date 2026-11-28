using AwesomeAssertions;
using ClinicFlow.Application.Patients.Queries.Shared;

namespace ClinicFlow.Application.Tests.Patients.Queries.Shared;

public class MatchMaskingTests
{
    [Fact]
    public void MaskEmail_ShouldReturnNull_WhenEmailIsNull()
    {
        // Arrange & Act
        var result = MatchMasking.MaskEmail(null);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    public void MaskEmail_ShouldReturnMaskedPlaceholder_WhenEmailHasNoLocalPart(string email)
    {
        // Arrange & Act
        var result = MatchMasking.MaskEmail(email);

        // Assert
        result.Should().Be("***");
    }

    [Fact]
    public void MaskEmail_ShouldMaskLocalPart_WhenEmailIsValid()
    {
        // Arrange
        const string email = "abcd@example.com";

        // Act
        var result = MatchMasking.MaskEmail(email);

        // Assert
        result.Should().Be("ab***@example.com");
    }

    [Fact]
    public void MaskPhone_ShouldReturnNull_WhenPhoneIsNull()
    {
        // Arrange & Act
        var result = MatchMasking.MaskPhone(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void MaskPhone_ShouldReturnMaskedPlaceholder_WhenDigitsAreFourOrFewer()
    {
        // Arrange
        string phone = "1234";

        // Act
        var result = MatchMasking.MaskPhone(phone);

        // Assert
        result.Should().Be("****");
    }

    [Fact]
    public void MaskPhone_ShouldReturnLastFourDigits_WhenDigitsExceedFour()
    {
        // Arrange
        string phone = "123456";

        // Act
        var result = MatchMasking.MaskPhone(phone);

        // Assert
        result.Should().Be("****3456");
    }
}

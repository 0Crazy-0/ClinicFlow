using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Tests.Common;

public class StringNormalizerTests
{
    [Fact]
    public void Canonicalize_ShouldLowercaseCollapseWhitespaceAndStripDiacritics()
    {
        // Arrange
        var input = "  CAFÉ   Naïve-Façade ";

        // Act
        var result = StringNormalizer.Canonicalize(input);

        // Assert
        result.Should().Be("cafe naive-facade");
    }

    [Theory]
    [InlineData("Façade   Naïve")]
    [InlineData("FAÇADE NAÏVE")]
    [InlineData("  façade   naïve  ")]
    [InlineData("Fac\u0327ade Nai\u0308ve")]
    [InlineData("facade naive")]
    public void Canonicalize_ShouldReturnCanonicalRepresentation_WhenValueContainsFormatting(
        string input
    )
    {
        // Arrange
        var expected = "facade naive";

        // Act
        var result = StringNormalizer.Canonicalize(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Canonicalize_ShouldThrowDomainValidationException_WhenValueIsNullOrWhiteSpace(
        string? value
    )
    {
        // Arrange & Act
        var act = () => StringNormalizer.Canonicalize(value!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }
}

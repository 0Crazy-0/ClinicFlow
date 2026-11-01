using AwesomeAssertions;
using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Tests.Common;

public class ActivationCodeGeneratorTests
{
    [Fact]
    public void Generate_ShouldReturnEightDigitNumericCode()
    {
        // Arrange & Act
        var codes = Enumerable
            .Range(0, 1_000)
            .Select(_ => ActivationCodeGenerator.Generate())
            .ToList();

        // Assert
        codes.Should().OnlyContain(code => code.Length == 8 && code.All(char.IsAsciiDigit));
        codes.Distinct().Should().HaveCountGreaterThan(1);
    }
}

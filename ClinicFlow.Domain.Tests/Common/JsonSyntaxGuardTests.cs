using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Tests.Common;

public class JsonSyntaxGuardTests
{
    [Theory]
    [InlineData("{}")]
    [InlineData("""{"heartRate":72}""")]
    [InlineData("[1,2,3]")]
    public void EnsureValidJson_ShouldNotThrow_WhenJsonIsWellFormed(string json)
    {
        // Act
        var act = () => JsonSyntaxGuard.EnsureValidJson(json);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("{invalid}")]
    [InlineData("{")]
    [InlineData("not json67")]
    [InlineData("""{"a":1,}""")]
    [InlineData("""{"a": // comentario""")]
    [InlineData("undefined")]
    [InlineData("""{"a":1} {"b":2}""")]
    public void EnsureValidJson_ShouldThrowException_WhenJsonIsMalformed(string json)
    {
        // Act
        var act = () => JsonSyntaxGuard.EnsureValidJson(json);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidFormat);
    }
}

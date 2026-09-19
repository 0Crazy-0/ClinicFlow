using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Tests.Entities;

public class DynamicClinicalDetailTests
{
    [Fact]
    public void Create_ShouldCreateDetail_WhenPayloadIsValid()
    {
        // Act
        var detail = DynamicClinicalDetail.Create("CARDIO_01", """{"heartRate":72}""");

        // Assert
        detail.TemplateCode.Should().Be("CARDIO_01");
        detail.JsonDataPayload.Should().Be("""{"heartRate":72}""");
    }

    [Fact]
    public void Create_ShouldCreateDetail_WhenPayloadIsEmptyObject()
    {
        // Act
        var detail = DynamicClinicalDetail.Create("CARDIO_01", "{}");

        // Assert
        detail.TemplateCode.Should().Be("CARDIO_01");
        detail.JsonDataPayload.Should().Be("{}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenTemplateCodeIsEmpty(string? templateCode)
    {
        // Act
        var act = () => DynamicClinicalDetail.Create(templateCode!, """{"heartRate":72}""");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenPayloadIsEmpty(string? payload)
    {
        // Act
        var act = () => DynamicClinicalDetail.Create("CARDIO_01", payload!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData("{invalid}")]
    [InlineData("{")]
    [InlineData("not json67")]
    [InlineData("""{"a":1,}""")]
    [InlineData("""{"a": // comentario""")]
    [InlineData("undefined")]
    [InlineData("""{"a":1} {"b":2}""")]
    public void Create_ShouldThrowException_WhenPayloadIsNotValidJson(string payload)
    {
        // Act
        var act = () => DynamicClinicalDetail.Create("CARDIO_01", payload);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidFormat);
    }
}

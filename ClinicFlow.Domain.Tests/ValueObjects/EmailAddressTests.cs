using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class EmailAddressTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenValueIsEmpty(string? value)
    {
        // Arrange & Act
        var act = () => EmailAddress.Create(value!);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@domain")]
    [InlineData("user @mail.com")]
    public void Create_ShouldThrowException_WhenFormatIsInvalid(string value)
    {
        // Arrange & Act
        var act = () => EmailAddress.Create(value);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEmailFormat);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenValueIsTooLong()
    {
        // Arrange
        var domain = "@example.com";
        var value = new string('a', EmailAddress.MaximumLength - domain.Length + 1) + domain;

        // Act
        var act = () => EmailAddress.Create(value);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValueLengthEqualsMaximumLength()
    {
        // Arrange
        var domain = "@example.com";
        var value = new string('a', EmailAddress.MaximumLength - domain.Length) + domain;

        // Act
        var email = EmailAddress.Create(value);

        // Assert
        email.Value.Should().Be(value);
    }

    [Fact]
    public void Create_ShouldNormalize_ToLowerInvariant()
    {
        // Arrange & Act
        var email = EmailAddress.Create("User@MAIL.COM");

        // Assert
        email.Value.Should().Be("user@mail.com");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var email = EmailAddress.Create("  user@mail.com  ");

        // Assert
        email.Value.Should().Be("user@mail.com");
    }

    [Theory]
    [InlineData("simple@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@domain.co")]
    public void Create_ShouldSucceed_WithValidEmail(string value)
    {
        // Arrange & Act
        var email = EmailAddress.Create(value);

        // Assert
        email.Value.Should().Be(value.Trim().ToLowerInvariant());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var email = EmailAddress.Create("test@example.com");

        // Act & Assert
        email.ToString().Should().Be(email.Value);
    }

    // Equality
    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var e1 = EmailAddress.Create("test@example.com");
        var e2 = EmailAddress.Create("test@example.com");
        var e3 = EmailAddress.Create("other@example.com");

        // Assert
        (e1 == e2)
            .Should()
            .BeTrue();
        (e1 != e3).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnUnderlyingValue()
    {
        // Arrange
        var email = EmailAddress.Create("test@test.com").Value;

        // Act
        string result = email;

        //Assert
        result.Should().Be("test@test.com");
    }
}

using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class EmailAddressTests
{
    // Create
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenValueIsEmpty(string? value)
    {
        // Arrange & Act
        var act = () => EmailAddress.Create(value!);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Email address cannot be empty.");
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
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Invalid email address format.");
    }

    // Create
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

    // ToString
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
        // Arrange
        var e1 = EmailAddress.Create("test@example.com");
        var e2 = EmailAddress.Create("test@example.com");
        var e3 = EmailAddress.Create("other@example.com");

        // Act & Assert
        (e1 == e2).Should().BeTrue();
        (e1 != e3).Should().BeTrue();
    }
}

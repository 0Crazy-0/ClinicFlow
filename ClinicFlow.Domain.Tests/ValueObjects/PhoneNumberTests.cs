using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenValueIsEmpty(string? value)
    {
        // Arrange & Act
        var act = () => PhoneNumber.Create(value!);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("12-34")]
    public void Create_ShouldThrowException_WhenFormatIsInvalid(string value)
    {
        // Arrange & Act
        var act = () => PhoneNumber.Create(value);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidPhoneFormat);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenValueIsTooLong()
    {
        // Arrange
        var value = new string('1', PhoneNumber.MaximumLength + 1);

        // Act
        var act = () => PhoneNumber.Create(value);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValueLengthEqualsMinimumLength()
    {
        // Arrange
        var value = new string('1', PhoneNumber.MinimumLength);

        // Act
        var phone = PhoneNumber.Create(value);

        // Assert
        phone.Value.Should().Be(value);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValueLengthEqualsMaximumLength()
    {
        // Arrange
        var value = "+" + new string('1', PhoneNumber.MaximumLength - 1);

        // Act
        var phone = PhoneNumber.Create(value);

        // Assert
        phone.Value.Should().Be(value);
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var phone = PhoneNumber.Create("  +1234567890  ");

        // Assert
        phone.Value.Should().Be("+1234567890");
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("1234567890")]
    [InlineData("+1 (234) 567-8901")]
    [InlineData("123-456-7890")]
    public void Create_ShouldSucceed_WithValidFormats(string value)
    {
        // Arrange & Act
        var phone = PhoneNumber.Create(value);

        // Assert
        phone.Value.Should().Be(value.Trim());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var phone = PhoneNumber.Create("+1234567890");

        // Act & Assert
        phone.ToString().Should().Be(phone.Value);
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var p1 = PhoneNumber.Create("+1234567890");
        var p2 = PhoneNumber.Create("+1234567890");
        var p3 = PhoneNumber.Create("+0987654321");

        // Assert
        (p1 == p2)
            .Should()
            .BeTrue();
        (p1 != p3).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnUnderlyingValue()
    {
        // Arrangue
        var phoneNumber = PhoneNumber.Create("+1234567890");

        // Act
        string result = phoneNumber;

        // Assert
        result.Should().Be("+1234567890");
    }
}

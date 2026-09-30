using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Common;

public class GuardTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void NotEmpty_ShouldThrowException_WhenValueIsEmpty()
    {
        // Arrange & Act
        var act = () => Guard.NotEmpty(Guid.Empty);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void NotEmpty_ShouldNotThrow_WhenValueIsProvided()
    {
        // Act
        var act = () => Guard.NotEmpty(Guid.CreateVersion7());

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotDefault_ShouldThrowException_WhenValueIsDefault()
    {
        // Arrange & Act
        var act = () => Guard.NotDefault(default);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void NotDefault_ShouldNotThrow_WhenValueIsProvided()
    {
        // Arrange & Act
        var act = () => Guard.NotDefault(_fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotNullOrWhiteSpace_ShouldThrowException_WhenValueIsEmpty(string? value)
    {
        // Arrange & Act
        var act = () => Guard.NotNullOrWhiteSpace(value);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void NotNullOrWhiteSpace_ShouldNotThrow_WhenValueIsProvided()
    {
        // Arrange & Act
        var act = () => Guard.NotNullOrWhiteSpace("string");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotNull_ShouldThrowException_WhenValueIsNull()
    {
        // Arrange & Act
        var act = () => Guard.NotNull<string>(null);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void NotNull_ShouldNotThrow_WhenValueIsProvided()
    {
        // Arrange & Act
        var act = () => Guard.NotNull("string");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void IsDefined_ShouldThrowException_WhenValueIsNotDefined()
    {
        // Arrange & Act
        var act = () => Guard.IsDefined((DayOfWeek)999);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void IsDefined_ShouldNotThrow_WhenValueIsDefined()
    {
        // Arrange & Act
        var act = () => Guard.IsDefined(DayOfWeek.Monday);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void IsDefined_ShouldThrowException_WhenNullableValueIsNotDefined()
    {
        // Arrange
        DayOfWeek? undefinedDay = (DayOfWeek)999;

        // Act
        var act = () => Guard.IsDefined(undefinedDay);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void IsDefined_ShouldNotThrow_WhenNullableValueIsNull()
    {
        // Arrange & Act
        var act = () => Guard.IsDefined((DayOfWeek?)null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void IsDefined_ShouldNotThrow_WhenNullableValueIsDefined()
    {
        // Arrange & Act
        var act = () => Guard.IsDefined((DayOfWeek?)DayOfWeek.Monday);

        // Assert
        act.Should().NotThrow();
    }
}

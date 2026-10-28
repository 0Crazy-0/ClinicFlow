using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class PersonNameTests
{
    [Theory]
    [InlineData(null, "six", "Garcia")]
    [InlineData("Jose", "seven", null)]
    public void Create_ShouldThrowValueRequired_WhenRequiredPartIsNull(
        string? firstName,
        string? middleName,
        string? firstSurname
    )
    {
        // Arrange
        var act = () => PersonName.Create(firstName!, middleName, firstSurname!);

        // Act & Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowValueRequired_WhenFirstNameIsBlank(string firstName)
    {
        // Arrange
        var act = () => PersonName.Create(firstName, null, "Garcia");

        // Act & Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowValueRequired_WhenFirstSurnameIsBlank(string firstSurname)
    {
        // Arrange & Assert
        var act = () => PersonName.Create("Jose", null, firstSurname);

        // Act
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowValueRequired_WhenMiddleNameProvidedBlank()
    {
        // Arrange & Act
        var act = () => PersonName.Create("Jose", "   ", "Garcia");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowValueTooShort_WhenFirstNameTooShort()
    {
        // Arrange
        var tooShort = new string('A', PersonName.MinimumLength - 1);

        // Act
        var act = () => PersonName.Create(tooShort, null, "Garcia");

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Create_ShouldThrowValueTooShort_WhenFirstSurnameTooShort()
    {
        // Arrange
        var tooShort = new string('A', PersonName.MinimumLength - 1);

        // Act
        var act = () => PersonName.Create("Jose", null, tooShort);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Create_ShouldThrowValueTooShort_WhenMiddleNameProvidedTooShort()
    {
        // Arrange
        var tooShort = new string('A', PersonName.MinimumLength - 1);

        // Act
        var act = () => PersonName.Create("Jose", tooShort, "Garcia");

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Create_ShouldThrowValueTooLong_WhenFirstNameTooLong()
    {
        // Arrange
        var tooLong = new string('A', PersonName.MaximumLength + 1);

        // Act
        var act = () => PersonName.Create(tooLong, null, "Garcia");

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Create_ShouldThrowValueTooLong_WhenMiddleNameTooLong()
    {
        // Arrange
        var tooLong = new string('A', PersonName.MaximumLength + 1);

        // Act
        var actMiddle = () => PersonName.Create("Jose", tooLong, "Garcia");

        // Assert
        actMiddle
            .Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenFirstNameLengthEqualsMinimumLength()
    {
        // Arrange
        var minimum = new string('A', PersonName.MinimumLength);

        // Act
        var name = PersonName.Create(minimum, null, "Garcia");

        // Assert
        name.FirstName.Should().Be(minimum);
        name.FirstSurname.Should().Be("Garcia");
    }

    [Fact]
    public void Create_ShouldSucceed_WhenFirstSurnameLengthEqualsMinimumLength()
    {
        // Arrange
        var minimum = new string('A', PersonName.MinimumLength);

        // Act
        var name = PersonName.Create("Jose", null, minimum);

        // Assert
        name.FirstName.Should().Be("Jose");
        name.FirstSurname.Should().Be(minimum);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenMiddleNameLengthEqualsMinimumLength()
    {
        // Arrange
        var minimum = new string('A', PersonName.MinimumLength);

        // Act
        var name = PersonName.Create("Jose", minimum, "Garcia");

        // Assert
        name.FirstName.Should().Be("Jose");
        name.MiddleName.Should().Be(minimum);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenRequiredPartLengthEqualsMaximumLength()
    {
        // Arrange
        var maximum = new string('A', PersonName.MaximumLength);

        // Act
        var name = PersonName.Create(maximum, null, "Garcia");

        // Assert
        name.FirstName.Should().Be(maximum);
        name.FirstSurname.Should().Be("Garcia");
    }

    [Fact]
    public void Create_ShouldSucceed_WhenFirstSurnameLengthEqualsMaximumLength()
    {
        // Arrange
        var maximum = new string('A', PersonName.MaximumLength);

        // Act
        var name = PersonName.Create("Jose", null, maximum);

        // Assert
        name.FirstName.Should().Be("Jose");
        name.FirstSurname.Should().Be(maximum);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenMiddleNameLengthEqualsMaximumLength()
    {
        // Arrange
        var maximum = new string('A', PersonName.MaximumLength);

        // Act
        var name = PersonName.Create("Jose", maximum, "Garcia");

        // Assert
        name.FirstName.Should().Be("Jose");
        name.MiddleName.Should().Be(maximum);
    }

    [Fact]
    public void Create_ShouldPreserveCasingAndAccents_WhileSearchNameNormalizes()
    {
        // Act
        var name = PersonName.Create("José", null, "García-Márquez");

        // Assert
        name.FirstName.Should().Be("José");
        name.MiddleName.Should().BeNull();
        name.FirstSurname.Should().Be("García-Márquez");
        name.SearchName.Should().Be("jose garcia-marquez");
    }

    [Fact]
    public void Create_ShouldCollapseInnerWhitespaceAndTrim()
    {
        // Act
        var name = PersonName.Create("  Ana   María ", null, "DE LA Cruz ");

        // Assert
        name.FirstName.Should().Be("Ana María");
        name.MiddleName.Should().BeNull();
        name.FirstSurname.Should().Be("DE LA Cruz");
        name.SearchName.Should().Be("ana maria de la cruz");
    }

    [Fact]
    public void Create_ShouldCleanProvidedMiddleName()
    {
        // Act
        var name = PersonName.Create("Ana", "  María  ", "Garcia");

        // Assert
        name.MiddleName.Should().Be("María");
    }

    [Fact]
    public void SearchName_ShouldExcludeMiddleName()
    {
        // Arrange & Act & Assert
        PersonName.Create("Ana", "María", "Garcia").SearchName.Should().Be("ana garcia");
        PersonName.Create("Ana", null, "Garcia").SearchName.Should().Be("ana garcia");
        PersonName.Create("Ana", null, "García").SearchName.Should().Be("ana garcia");
    }

    [Fact]
    public void NormalizeForSearch_ShouldLowercaseCollapseAndStripDiacritics()
    {
        // Arrange & Act & Assert
        PersonName
            .NormalizeForSearch("  JOSÉ   García-Márquez ")
            .Should()
            .Be("jose garcia-marquez");
    }

    [Fact]
    public void ToString_ShouldJoinParts()
    {
        // Arrange & Act & Assert
        PersonName.Create("Ana", "María", "Garcia").ToString().Should().Be("Ana María Garcia");
        PersonName.Create("Ana", null, "Garcia").ToString().Should().Be("Ana Garcia");
    }

    [Fact]
    public void Equality_ShouldRequireExactSpelling_RegardlessOfSearchName()
    {
        // Arrange
        var n1 = PersonName.Create("Jose", null, "Garcia");
        var n2 = PersonName.Create("Jose", null, "Garcia");
        var accented = PersonName.Create("José", null, "Garcia");
        var n3 = PersonName.Create("John", null, "Doe");

        // Assert
        (n1 == n2)
            .Should()
            .BeTrue();

        (n1 != accented).Should().BeTrue();

        accented.SearchName.Should().Be(n1.SearchName);

        (n1 != n3).Should().BeTrue();
    }
}

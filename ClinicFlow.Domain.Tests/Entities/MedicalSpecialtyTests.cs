using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class MedicalSpecialtyTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Create_ShouldCreateSpecialty_WhenValidParameters()
    {
        // Arrange
        var name = "Cardiology";
        var description = "Heart specialty";
        var typicalDuration = 30;
        var minCancellationHours = 24;

        // Act
        var specialty = MedicalSpecialty.Create(
            name,
            description,
            typicalDuration,
            minCancellationHours
        );

        // Assert
        specialty.Should().NotBeNull();
        specialty.Name.Should().Be(name);
        specialty.Description.Should().Be(description);
        specialty.TypicalDurationMinutes.Should().Be(typicalDuration);
        specialty.MinCancellationHours.Should().Be(minCancellationHours);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange & Act
        var act = () => MedicalSpecialty.Create(name!, "Description", 30, 24);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenDescriptionIsEmpty(string? description)
    {
        // Arrange & Act
        var act = () => MedicalSpecialty.Create("Cardiology", description!, 30, 24);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowException_WhenDurationIsZeroOrNegative(int duration)
    {
        // Arrange & Act
        var act = () => MedicalSpecialty.Create("Cardiology", "Description", duration, 24);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenMinCancellationHoursIsNegative()
    {
        // Arrange & Act
        var act = () => MedicalSpecialty.Create("Cardiology", "Description", 30, -1);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateAllFields_WhenValidParameters()
    {
        // Arrange
        var specialty = CreateSpecialty();
        var newName = "Dermatology";
        var newDescription = "Skin specialty";
        var newDuration = 45;
        var newCancellationHours = 12;

        // Act
        specialty.UpdateDetails(newName, newDescription, newDuration, newCancellationHours);

        // Assert
        specialty.Name.Should().Be(newName);
        specialty.Description.Should().Be(newDescription);
        specialty.TypicalDurationMinutes.Should().Be(newDuration);
        specialty.MinCancellationHours.Should().Be(newCancellationHours);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var specialty = CreateSpecialty();

        // Act
        var act = () => specialty.UpdateDetails(name!, "Description", 30, 24);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_ShouldThrowException_WhenDescriptionIsEmpty(string? description)
    {
        // Arrange
        var specialty = CreateSpecialty();

        // Act
        var act = () => specialty.UpdateDetails("Cardiology", description!, 30, 24);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateDetails_ShouldThrowException_WhenDurationIsZeroOrNegative(int duration)
    {
        // Arrange
        var specialty = CreateSpecialty();

        // Act
        var act = () => specialty.UpdateDetails("Cardiology", "Description", duration, 24);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Fact]
    public void UpdateDetails_ShouldThrowException_WhenMinCancellationHoursIsNegative()
    {
        // Arrange
        var specialty = CreateSpecialty();

        // Act
        var act = () => specialty.UpdateDetails("Cardiology", "Description", 30, -1);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }

    [Fact]
    public void Reactivate_ShouldUndoDeletion_WhenInactive()
    {
        // Arrange
        var specialty = CreateSpecialty();
        specialty.Deactivate(false);

        // Act
        specialty.Reactivate();

        // Assert
        specialty.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Reactivate_ShouldThrowException_WhenAlreadyActive()
    {
        // Arrange
        var specialty = CreateSpecialty();

        // Act & Assert
        specialty
            .Invoking(s => s.Reactivate())
            .Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.AlreadyActive);
    }

    [Fact]
    public void Deactivate_ShouldMarkAsDeleted_WhenNoActiveDoctors()
    {
        // Arrange
        var specialty = CreateSpecialty();

        // Act
        specialty.Deactivate(false);

        // Assert
        specialty.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldThrowException_WhenAlreadyInactive()
    {
        // Arrange
        var specialty = CreateSpecialty();
        specialty.Deactivate(false);

        // Act
        var act = () => specialty.Deactivate(false);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.AlreadyInactive);
    }

    [Fact]
    public void Deactivate_ShouldThrowException_WhenHasActiveDoctors()
    {
        // Arrange
        var specialty = CreateSpecialty();

        // Act
        var act = () => specialty.Deactivate(true);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.HasActiveDoctors);
    }

    [Fact]
    public void IsCancellationAllowed_ShouldReturnTrue_WhenSufficientNotice() =>
        MedicalSpecialty
            .Create("Cardiology", "Description", 30, 24)
            .IsCancellationAllowed(
                _fakeTime.GetUtcNow().UtcDateTime.AddHours(48).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            )
            .Should()
            .BeTrue(); // Arrange & Act & Assert

    [Fact]
    public void IsCancellationAllowed_ShouldReturnFalse_WhenInsufficientNotice() =>
        MedicalSpecialty
            .Create("Cardiology", "Description", 30, 24)
            .IsCancellationAllowed(
                _fakeTime.GetUtcNow().UtcDateTime.AddHours(2).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            )
            .Should()
            .BeFalse(); // Arrange & Act & Assert

    private static MedicalSpecialty CreateSpecialty() =>
        MedicalSpecialty.Create("Cardiology", "Heart specialty", 30, 24);
}

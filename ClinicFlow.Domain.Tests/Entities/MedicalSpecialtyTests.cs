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
        specialty.TypicalDuration.Minutes.Should().Be(typicalDuration);
        specialty.CancellationPolicy.Hours.Should().Be(minCancellationHours);
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
    [InlineData(9)]
    [InlineData(12)]
    [InlineData(95)]
    public void Create_ShouldThrowException_WhenDurationIsInvalid(int duration)
    {
        // Arrange & Act
        var act = () => MedicalSpecialty.Create("Cardiology", "Description", duration, 24);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.InvalidEncounterDuration);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(13)]
    [InlineData(23)]
    public void Create_ShouldThrowException_WhenMinCancellationHoursIsInvalid(int hours)
    {
        // Arrange & Act
        var act = () => MedicalSpecialty.Create("Cardiology", "Description", 30, hours);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.InvalidCancellationLimit);
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
        specialty.TypicalDuration.Minutes.Should().Be(newDuration);
        specialty.CancellationPolicy.Hours.Should().Be(newCancellationHours);
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
    [InlineData(11)]
    [InlineData(39)]
    [InlineData(81)]
    public void UpdateDetails_ShouldThrowException_WhenDurationIsInvalid(int duration)
    {
        // Arrange
        var specialty = CreateSpecialty();

        // Act
        var act = () => specialty.UpdateDetails("Cardiology", "Description", duration, 24);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.InvalidEncounterDuration);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(49)]
    [InlineData(71)]
    public void UpdateDetails_ShouldThrowException_WhenMinCancellationHoursIsInvalid(int hours)
    {
        // Arrange
        var specialty = CreateSpecialty();

        // Act
        var act = () => specialty.UpdateDetails("Cardiology", "Description", 30, hours);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalSpecialty.InvalidCancellationLimit);
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

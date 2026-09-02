using ClinicFlow.Application.FamilyMemberships.Commands.ChangeAccessLevel;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.FamilyMemberships.Commands.ChangeAccessLevel;

public class ChangeAccessLevelCommandValidatorTests
{
    private readonly ChangeAccessLevelCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new ChangeAccessLevelCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            FamilyMembershipAccessLevel.Restricted
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRequesterUserIdIsEmpty()
    {
        // Arrange
        var command = new ChangeAccessLevelCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            FamilyMembershipAccessLevel.Restricted
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.RequesterUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTargetUserIdIsEmpty()
    {
        // Arrange
        var command = new ChangeAccessLevelCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            Guid.CreateVersion7(),
            FamilyMembershipAccessLevel.Restricted
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var command = new ChangeAccessLevelCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.Empty,
            FamilyMembershipAccessLevel.Restricted
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNewAccessLevelIsNotDefined()
    {
        // Arrange
        var command = new ChangeAccessLevelCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            (FamilyMembershipAccessLevel)999
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.NewAccessLevel)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}

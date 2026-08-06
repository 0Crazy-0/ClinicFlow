using ClinicFlow.Application.Patients.Commands.RemoveFamilyMember;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.RemoveFamilyMember;

public class RemoveFamilyMemberCommandValidatorTests
{
    private readonly RemoveFamilyMemberCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvided()
    {
        // Arrange
        var command = new RemoveFamilyMemberCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Self
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var command = new RemoveFamilyMemberCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            PatientRelationship.Self
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new RemoveFamilyMemberCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            PatientRelationship.Self
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorRelationshipIsInvalid()
    {
        // Arrange
        var command = new RemoveFamilyMemberCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            (PatientRelationship)99
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorRelationship)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}

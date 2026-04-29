using ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.AddCompleteFamilyMember;

public class AddCompleteFamilyMemberCommandValidatorTests
{
    private readonly AddCompleteFamilyMemberCommandValidator _sut;

    public AddCompleteFamilyMemberCommandValidatorTests()
    {
        _sut = new AddCompleteFamilyMemberCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Child
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRelationshipIsInvalidEnum()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555",
            (PatientRelationship)999
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Relationship)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}

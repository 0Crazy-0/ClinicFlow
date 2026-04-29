using ClinicFlow.Application.Patients.Commands.AddFamilyMember;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.AddFamilyMember;

public class AddFamilyMemberCommandValidatorTests
{
    private readonly AddFamilyMemberCommandValidator _sut;

    public AddFamilyMemberCommandValidatorTests()
    {
        _sut = new AddFamilyMemberCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
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
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
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

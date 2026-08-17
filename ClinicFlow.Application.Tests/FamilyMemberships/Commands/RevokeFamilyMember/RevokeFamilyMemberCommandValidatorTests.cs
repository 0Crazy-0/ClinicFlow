using ClinicFlow.Application.FamilyMemberships.Commands.RevokeFamilyMember;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.FamilyMemberships.Commands.RevokeFamilyMember;

public class RevokeFamilyMemberCommandValidatorTests
{
    private readonly RevokeFamilyMemberCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new RevokeFamilyMemberCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenOwnerUserIdIsEmpty()
    {
        // Arrange
        var command = new RevokeFamilyMemberCommand(Guid.Empty, Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.OwnerUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var command = new RevokeFamilyMemberCommand(Guid.CreateVersion7(), Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

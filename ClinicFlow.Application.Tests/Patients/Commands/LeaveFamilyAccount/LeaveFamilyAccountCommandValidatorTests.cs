using ClinicFlow.Application.Patients.Commands.LeaveFamilyAccount;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.LeaveFamilyAccount;

public class LeaveFamilyAccountCommandValidatorTests
{
    private readonly LeaveFamilyAccountCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvided()
    {
        // Arrange
        var command = new LeaveFamilyAccountCommand(Guid.CreateVersion7(), Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var command = new LeaveFamilyAccountCommand(Guid.Empty, Guid.CreateVersion7());

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
        var command = new LeaveFamilyAccountCommand(Guid.CreateVersion7(), Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

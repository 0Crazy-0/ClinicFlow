using ClinicFlow.Application.Patients.Commands.ClosePatientAccount;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.ClosePatientAccount;

public class ClosePatientAccountCommandValidatorTests
{
    private readonly ClosePatientAccountCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldBeValid_WhenUserIdIsProvided()
    {
        // Arrange
        var command = new ClosePatientAccountCommand(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new ClosePatientAccountCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

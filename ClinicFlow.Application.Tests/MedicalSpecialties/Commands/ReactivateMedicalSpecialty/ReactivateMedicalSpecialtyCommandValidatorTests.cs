using ClinicFlow.Application.MedicalSpecialties.Commands.ReactivateMedicalSpecialty;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Commands.ReactivateMedicalSpecialty;

public class ReactivateMedicalSpecialtyCommandValidatorTests
{
    private readonly ReactivateMedicalSpecialtyCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new ReactivateMedicalSpecialtyCommand(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenSpecialtyIdIsEmpty()
    {
        // Arrange
        var command = new ReactivateMedicalSpecialtyCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SpecialtyId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

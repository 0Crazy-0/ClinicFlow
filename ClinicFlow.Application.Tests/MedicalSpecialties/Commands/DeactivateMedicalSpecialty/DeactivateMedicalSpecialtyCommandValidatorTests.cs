using ClinicFlow.Application.MedicalSpecialties.Commands.DeactivateMedicalSpecialty;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Commands.DeactivateMedicalSpecialty;

public class DeactivateMedicalSpecialtyCommandValidatorTests
{
    private readonly DeactivateMedicalSpecialtyCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new DeactivateMedicalSpecialtyCommand(Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenSpecialtyIdIsEmpty()
    {
        // Arrange
        var command = new DeactivateMedicalSpecialtyCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SpecialtyId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

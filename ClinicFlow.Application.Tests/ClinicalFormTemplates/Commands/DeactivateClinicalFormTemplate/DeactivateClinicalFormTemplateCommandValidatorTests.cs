using ClinicFlow.Application.ClinicalFormTemplates.Commands.DeactivateClinicalFormTemplate;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.DeactivateClinicalFormTemplate;

public class DeactivateClinicalFormTemplateCommandValidatorTests
{
    private readonly DeactivateClinicalFormTemplateCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new DeactivateClinicalFormTemplateCommand(Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenTemplateIdIsEmpty()
    {
        // Arrange
        var command = new DeactivateClinicalFormTemplateCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TemplateId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

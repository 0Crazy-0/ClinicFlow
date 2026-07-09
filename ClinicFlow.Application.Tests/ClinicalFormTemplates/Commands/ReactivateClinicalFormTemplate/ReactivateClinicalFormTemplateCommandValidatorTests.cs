using ClinicFlow.Application.ClinicalFormTemplates.Commands.ReactivateClinicalFormTemplate;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.ReactivateClinicalFormTemplate;

public class ReactivateClinicalFormTemplateCommandValidatorTests
{
    private readonly ReactivateClinicalFormTemplateCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new ReactivateClinicalFormTemplateCommand(Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenTemplateIdIsEmpty()
    {
        // Arrange
        var command = new ReactivateClinicalFormTemplateCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TemplateId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

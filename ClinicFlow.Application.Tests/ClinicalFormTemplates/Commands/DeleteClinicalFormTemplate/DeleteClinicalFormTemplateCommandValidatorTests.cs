using ClinicFlow.Application.ClinicalFormTemplates.Commands.DeleteClinicalFormTemplate;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.ClinicalFormTemplates.Commands.DeleteClinicalFormTemplate;

public class DeleteClinicalFormTemplateCommandValidatorTests
{
    private readonly DeleteClinicalFormTemplateCommandValidator _sut;

    public DeleteClinicalFormTemplateCommandValidatorTests()
    {
        _sut = new DeleteClinicalFormTemplateCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenTemplateIdIsProvided()
    {
        // Arrange
        var command = new DeleteClinicalFormTemplateCommand(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTemplateIdIsEmpty()
    {
        // Arrange
        var command = new DeleteClinicalFormTemplateCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TemplateId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

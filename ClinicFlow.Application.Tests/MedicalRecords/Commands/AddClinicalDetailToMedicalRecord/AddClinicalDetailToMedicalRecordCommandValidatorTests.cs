using ClinicFlow.Application.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;
using ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalRecords.Commands.AddClinicalDetailToMedicalRecord;

public class AddClinicalDetailToMedicalRecordCommandValidatorTests
{
    private readonly AddClinicalDetailToMedicalRecordCommandValidator _sut;

    public AddClinicalDetailToMedicalRecordCommandValidatorTests()
    {
        _sut = new AddClinicalDetailToMedicalRecordCommandValidator();
    }

    [Fact]
    public void Validate_GivenValidCommand_HasNoErrors()
    {
        // Arrange
        var command = new AddClinicalDetailToMedicalRecordCommand(Guid.NewGuid(), new DynamicClinicalDetailDto("vital-signs", "{}"));

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_GivenEmptyMedicalRecordId_HasError()
    {
        // Arrange
        var command = new AddClinicalDetailToMedicalRecordCommand(Guid.Empty, new DynamicClinicalDetailDto("vital-signs", "{}"));

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.MedicalRecordId).WithErrorMessage("Medical Record ID is required.");
    }

    [Fact]
    public void Validate_GivenNullDetail_HasError()
    {
        // Arrange
        var command = new AddClinicalDetailToMedicalRecordCommand(Guid.NewGuid(), null!);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Detail).WithErrorMessage("Clinical detail must be provided.");
    }

    [Theory]
    [InlineData("", "somedata")]
    [InlineData("   ", "somedata")]
    [InlineData(null, "somedata")]
    public void Validate_GivenEmptyTemplateCode_HasError(string? templateCode, string? payload)
    {
        // Arrange
        var command = new AddClinicalDetailToMedicalRecordCommand(Guid.NewGuid(), new DynamicClinicalDetailDto(templateCode!, payload!));

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Detail.TemplateCode)
              .WithErrorMessage("Template code is required for the clinical detail.");
    }

    [Theory]
    [InlineData("templateCode", "")]
    [InlineData("templateCode", "   ")]
    [InlineData("templateCode", null)]
    public void Validate_GivenEmptyPayload_HasError(string? templateCode, string? payload)
    {
        // Arrange
        var command = new AddClinicalDetailToMedicalRecordCommand(Guid.NewGuid(), new DynamicClinicalDetailDto(templateCode!, payload!));

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Detail.JsonDataPayload).WithErrorMessage("JSON data payload is required for the clinical detail.");
    }
}

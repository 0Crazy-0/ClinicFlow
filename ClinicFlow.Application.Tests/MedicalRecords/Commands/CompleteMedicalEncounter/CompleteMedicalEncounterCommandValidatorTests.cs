using ClinicFlow.Application.MedicalRecords.Commands.CompleteMedicalEncounter;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using DynamicClinicalDetailDto = (string TemplateCode, string JsonDataPayload);

namespace ClinicFlow.Application.Tests.MedicalRecords.Commands.CompleteMedicalEncounter;

public class CompleteMedicalEncounterCommandValidatorTests
{
    private readonly CompleteMedicalEncounterCommandValidator _sut;

    public CompleteMedicalEncounterCommandValidatorTests()
    {
        _sut = new CompleteMedicalEncounterCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CompleteMedicalEncounterCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Headache",
            [new DynamicClinicalDetailDto("vital-signs", "{}")]
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var command = new CompleteMedicalEncounterCommand(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Headache",
            []
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new CompleteMedicalEncounterCommand(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            "Headache",
            []
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DoctorId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new CompleteMedicalEncounterCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            "Headache",
            []
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenChiefComplaintIsEmpty()
    {
        // Arrange
        var command = new CompleteMedicalEncounterCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            string.Empty,
            []
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ChiefComplaint)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTemplateCodeIsEmpty()
    {
        // Arrange
        var command = new CompleteMedicalEncounterCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Headache",
            [new DynamicClinicalDetailDto(string.Empty, "{}")]
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor("Details[0].TemplateCode")
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenJsonDataPayloadIsEmpty()
    {
        // Arrange
        var command = new CompleteMedicalEncounterCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Headache",
            [new DynamicClinicalDetailDto("vital-signs", string.Empty)]
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor("Details[0].JsonDataPayload")
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }
}

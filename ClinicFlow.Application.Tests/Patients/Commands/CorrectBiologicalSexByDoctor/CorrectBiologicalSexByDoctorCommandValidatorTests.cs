using ClinicFlow.Application.Patients.Commands.CorrectBiologicalSexByDoctor;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.CorrectBiologicalSexByDoctor;

public class CorrectBiologicalSexByDoctorCommandValidatorTests
{
    private readonly CorrectBiologicalSexByDoctorCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CorrectBiologicalSexByDoctorCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            BiologicalSex.Female,
            "reason"
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
        var command = new CorrectBiologicalSexByDoctorCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            BiologicalSex.Female,
            "reason"
        );

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
        var command = new CorrectBiologicalSexByDoctorCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            BiologicalSex.Female,
            "reason"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCorrectedSexIsInvalid()
    {
        // Arrange
        var command = new CorrectBiologicalSexByDoctorCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            (BiologicalSex)999,
            "reason"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.CorrectedSex)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCorrectedSexIsUnspecified()
    {
        // Arrange
        var command = new CorrectBiologicalSexByDoctorCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            BiologicalSex.Unspecified,
            "reason"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.CorrectedSex)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenReasonIsEmpty(string? reason)
    {
        // Arrange
        var command = new CorrectBiologicalSexByDoctorCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            BiologicalSex.Female,
            reason!
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenReasonIsTooLong()
    {
        // Arrange
        var command = new CorrectBiologicalSexByDoctorCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            BiologicalSex.Female,
            new string('A', Patient.MaxCorrectionReasonLength + 1)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}

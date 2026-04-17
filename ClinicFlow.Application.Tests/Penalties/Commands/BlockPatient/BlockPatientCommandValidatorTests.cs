using ClinicFlow.Application.Penalties.Commands.BlockPatient;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Penalties.Commands.BlockPatient;

public class BlockPatientCommandValidatorTests
{
    private readonly BlockPatientCommandValidator _sut;

    public BlockPatientCommandValidatorTests()
    {
        _sut = new BlockPatientCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new BlockPatientCommand(
            Guid.NewGuid(),
            "Patient was rude to staff",
            BlockDuration.Minor
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
        var command = new BlockPatientCommand(Guid.Empty, "Block reason", BlockDuration.Minor);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenReasonIsEmpty(string? reason)
    {
        // Arrange
        var command = new BlockPatientCommand(Guid.NewGuid(), reason!, BlockDuration.Minor);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDurationIsInvalid()
    {
        // Arrange
        var command = new BlockPatientCommand(Guid.NewGuid(), "Block reason", (BlockDuration)999);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }
}

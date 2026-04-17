using ClinicFlow.Application.Penalties.Commands.RemovePenalty;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Penalties.Commands.RemovePenalty;

public class RemovePenaltyCommandValidatorTests
{
    private readonly RemovePenaltyCommandValidator _sut;

    public RemovePenaltyCommandValidatorTests()
    {
        _sut = new RemovePenaltyCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenPenaltyIdIsValid()
    {
        // Arrange
        var command = new RemovePenaltyCommand(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPenaltyIdIsEmpty()
    {
        // Arrange
        var command = new RemovePenaltyCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PenaltyId);
    }
}

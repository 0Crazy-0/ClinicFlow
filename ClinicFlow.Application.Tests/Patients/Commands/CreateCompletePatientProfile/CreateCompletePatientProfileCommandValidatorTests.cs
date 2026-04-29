using ClinicFlow.Application.Patients.Commands.CreateCompletePatientProfile;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.CreateCompletePatientProfile;

public class CreateCompletePatientProfileCommandValidatorTests
{
    private readonly CreateCompletePatientProfileCommandValidator _sut;

    public CreateCompletePatientProfileCommandValidatorTests()
    {
        _sut = new CreateCompletePatientProfileCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CreateCompletePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-30),
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

using ClinicFlow.Application.Patients.Commands.CreatePatientProfile;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.CreatePatientProfile;

public class CreatePatientProfileCommandValidatorTests
{
    private readonly CreatePatientProfileCommandValidator _sut;

    public CreatePatientProfileCommandValidatorTests()
    {
        _sut = new CreatePatientProfileCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-30)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

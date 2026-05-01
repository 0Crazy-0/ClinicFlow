using ClinicFlow.Application.Patients.Commands.CreateCompletePatientProfile;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Patients.Commands.CreateCompletePatientProfile;

public class CreateCompletePatientProfileCommandValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly CreateCompletePatientProfileCommandValidator _sut;

    public CreateCompletePatientProfileCommandValidatorTests()
    {
        _sut = new CreateCompletePatientProfileCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CreateCompletePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
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

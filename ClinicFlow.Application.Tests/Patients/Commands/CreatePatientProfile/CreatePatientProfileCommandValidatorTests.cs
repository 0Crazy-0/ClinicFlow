using ClinicFlow.Application.Patients.Commands.CreatePatientProfile;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Patients.Commands.CreatePatientProfile;

public class CreatePatientProfileCommandValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly CreatePatientProfileCommandValidator _sut;

    public CreatePatientProfileCommandValidatorTests()
    {
        _sut = new CreatePatientProfileCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

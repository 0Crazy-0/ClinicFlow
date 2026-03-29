using ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByPatient;

public class RescheduleByPatientCommandValidatorTests
{
    private readonly RescheduleByPatientCommandValidator _sut;

    public RescheduleByPatientCommandValidatorTests()
    {
        _sut = new RescheduleByPatientCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new RescheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

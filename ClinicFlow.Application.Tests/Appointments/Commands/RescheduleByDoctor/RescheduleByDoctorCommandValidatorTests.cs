using ClinicFlow.Application.Appointments.Commands.RescheduleByDoctor;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByDoctor;

public class RescheduleByDoctorCommandValidatorTests
{
    private readonly RescheduleByDoctorCommandValidator _sut;

    public RescheduleByDoctorCommandValidatorTests()
    {
        _sut = new RescheduleByDoctorCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new RescheduleByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

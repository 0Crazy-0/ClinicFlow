using ClinicFlow.Application.Doctors.Commands.ReactivateDoctorProfile;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Doctors.Commands.ReactivateDoctorProfile;

public class ReactivateDoctorProfileCommandValidatorTests
{
    private readonly ReactivateDoctorProfileCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new ReactivateDoctorProfileCommand(Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new ReactivateDoctorProfileCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DoctorId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

using ClinicFlow.Application.Doctors.Commands.SuspendDoctorProfile;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Doctors.Commands.SuspendDoctorProfile;

public class SuspendDoctorProfileCommandValidatorTests
{
    private readonly SuspendDoctorProfileCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new SuspendDoctorProfileCommand(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new SuspendDoctorProfileCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DoctorId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

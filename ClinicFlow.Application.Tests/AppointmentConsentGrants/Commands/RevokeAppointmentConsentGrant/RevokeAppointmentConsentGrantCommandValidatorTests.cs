using ClinicFlow.Application.AppointmentConsentGrants.Commands.RevokeAppointmentConsentGrant;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentConsentGrants.Commands.RevokeAppointmentConsentGrant;

public class RevokeAppointmentConsentGrantCommandValidatorTests
{
    private readonly RevokeAppointmentConsentGrantCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new RevokeAppointmentConsentGrantCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRequesterUserIdIsEmpty()
    {
        // Arrange
        var command = new RevokeAppointmentConsentGrantCommand(Guid.Empty, Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.RequesterUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenConsentGrantIdIsEmpty()
    {
        // Arrange
        var command = new RevokeAppointmentConsentGrantCommand(Guid.CreateVersion7(), Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsentGrantId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

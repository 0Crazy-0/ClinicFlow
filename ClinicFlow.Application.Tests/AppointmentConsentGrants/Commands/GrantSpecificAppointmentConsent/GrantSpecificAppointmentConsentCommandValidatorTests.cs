using ClinicFlow.Application.AppointmentConsentGrants.Commands.GrantSpecificAppointmentConsent;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentConsentGrants.Commands.GrantSpecificAppointmentConsent;

public class GrantSpecificAppointmentConsentCommandValidatorTests
{
    private readonly GrantSpecificAppointmentConsentCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new GrantSpecificAppointmentConsentCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
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
        var command = new GrantSpecificAppointmentConsentCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.RequesterUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var command = new GrantSpecificAppointmentConsentCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRecipientMembershipIdIsEmpty()
    {
        // Arrange
        var command = new GrantSpecificAppointmentConsentCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.Empty,
            Guid.CreateVersion7()
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.RecipientMembershipId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new GrantSpecificAppointmentConsentCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.Empty
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

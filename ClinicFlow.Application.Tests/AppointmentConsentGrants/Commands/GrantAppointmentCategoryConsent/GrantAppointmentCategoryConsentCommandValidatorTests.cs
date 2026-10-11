using ClinicFlow.Application.AppointmentConsentGrants.Commands.GrantAppointmentCategoryConsent;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentConsentGrants.Commands.GrantAppointmentCategoryConsent;

public class GrantAppointmentCategoryConsentCommandValidatorTests
{
    private readonly GrantAppointmentCategoryConsentCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new GrantAppointmentCategoryConsentCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            ProtectedCategory.MentalHealthCounseling
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
        var command = new GrantAppointmentCategoryConsentCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            ProtectedCategory.MentalHealthCounseling
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
        var command = new GrantAppointmentCategoryConsentCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            Guid.CreateVersion7(),
            ProtectedCategory.MentalHealthCounseling
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
        var command = new GrantAppointmentCategoryConsentCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.Empty,
            ProtectedCategory.MentalHealthCounseling
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.RecipientMembershipId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenProtectedCareCategoryIsInvalid()
    {
        // Arrange
        var command = new GrantAppointmentCategoryConsentCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            (ProtectedCategory)999
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ProtectedCareCategory)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}

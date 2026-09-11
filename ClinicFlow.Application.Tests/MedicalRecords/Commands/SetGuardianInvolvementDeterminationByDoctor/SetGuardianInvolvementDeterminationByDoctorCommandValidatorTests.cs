using ClinicFlow.Application.MedicalRecords.Commands.SetGuardianInvolvementDeterminationByDoctor;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalRecords.Commands.SetGuardianInvolvementDeterminationByDoctor;

public class SetGuardianInvolvementDeterminationByDoctorCommandValidatorTests
{
    private readonly SetGuardianInvolvementDeterminationByDoctorCommandValidator _sut;

    public SetGuardianInvolvementDeterminationByDoctorCommandValidatorTests()
    {
        _sut = new SetGuardianInvolvementDeterminationByDoctorCommandValidator();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_GivenValidCommand_HasNoErrors(bool guardianInvolvementDeemedAppropriate)
    {
        // Arrange
        var command = new SetGuardianInvolvementDeterminationByDoctorCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            guardianInvolvementDeemedAppropriate
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_GivenEmptyMedicalRecordId_HasError()
    {
        // Arrange
        var command = new SetGuardianInvolvementDeterminationByDoctorCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            true
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(c => c.MedicalRecordId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_GivenEmptyInitiatorUserId_HasError()
    {
        // Arrange
        var command = new SetGuardianInvolvementDeterminationByDoctorCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            true
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(c => c.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

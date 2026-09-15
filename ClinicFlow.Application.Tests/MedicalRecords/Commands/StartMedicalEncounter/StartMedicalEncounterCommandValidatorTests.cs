using ClinicFlow.Application.MedicalRecords.Commands.StartMedicalEncounter;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalRecords.Commands.StartMedicalEncounter;

public class StartMedicalEncounterCommandValidatorTests
{
    private readonly StartMedicalEncounterCommandValidator _sut;

    public StartMedicalEncounterCommandValidatorTests()
    {
        _sut = new StartMedicalEncounterCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new StartMedicalEncounterCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Headache",
            null
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new StartMedicalEncounterCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            "Headache",
            null
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DoctorId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new StartMedicalEncounterCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            "Headache",
            null
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenChiefComplaintIsEmpty(string? chiefComplaint)
    {
        // Arrange
        var command = new StartMedicalEncounterCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            chiefComplaint!,
            null
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ChiefComplaint)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }
}

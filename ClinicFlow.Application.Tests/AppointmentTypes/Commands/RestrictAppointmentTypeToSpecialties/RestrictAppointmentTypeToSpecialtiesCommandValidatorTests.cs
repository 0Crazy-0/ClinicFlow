using ClinicFlow.Application.AppointmentTypes.Commands.RestrictAppointmentTypeToSpecialties;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.RestrictAppointmentTypeToSpecialties;

public class RestrictAppointmentTypeToSpecialtiesCommandValidatorTests
{
    private readonly RestrictAppointmentTypeToSpecialtiesCommandValidator _sut;

    public RestrictAppointmentTypeToSpecialtiesCommandValidatorTests()
    {
        _sut = new RestrictAppointmentTypeToSpecialtiesCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new RestrictAppointmentTypeToSpecialtiesCommand(
            Guid.CreateVersion7(),
            [Guid.CreateVersion7(), Guid.CreateVersion7()]
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new RestrictAppointmentTypeToSpecialtiesCommand(
            Guid.Empty,
            [Guid.CreateVersion7()]
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSpecialtyIdsIsEmpty()
    {
        // Arrange
        var command = new RestrictAppointmentTypeToSpecialtiesCommand(Guid.CreateVersion7(), []);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SpecialtyIds)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSpecialtyIdsContainsEmptyGuid()
    {
        // Arrange
        var command = new RestrictAppointmentTypeToSpecialtiesCommand(
            Guid.CreateVersion7(),
            [Guid.CreateVersion7(), Guid.Empty]
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SpecialtyIds)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

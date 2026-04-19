using ClinicFlow.Application.AppointmentTypes.Queries.GetEligibleAppointmentTypes;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetEligibleAppointmentTypes;

public class GetEligibleAppointmentTypesQueryValidatorTests
{
    private readonly GetEligibleAppointmentTypesQueryValidator _sut;

    public GetEligibleAppointmentTypesQueryValidatorTests()
    {
        _sut = new GetEligibleAppointmentTypesQueryValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAgeIsZero()
    {
        // Arrange
        var query = new GetEligibleAppointmentTypesQuery(0);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAgeIsPositive()
    {
        // Arrange
        var query = new GetEligibleAppointmentTypesQuery(30);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAgeIsNegative()
    {
        // Arrange
        var query = new GetEligibleAppointmentTypesQuery(-1);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientAgeInYears);
    }
}

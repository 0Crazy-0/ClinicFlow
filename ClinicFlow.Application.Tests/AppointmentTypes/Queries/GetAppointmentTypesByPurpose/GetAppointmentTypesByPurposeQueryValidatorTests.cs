using ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByPurpose;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetAppointmentTypesByPurpose;

public class GetAppointmentTypesByPurposeQueryValidatorTests
{
    private readonly GetAppointmentTypesByPurposeQueryValidator _sut;

    public GetAppointmentTypesByPurposeQueryValidatorTests()
    {
        _sut = new GetAppointmentTypesByPurposeQueryValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenPurposeIsValid()
    {
        // Arrange
        var query = new GetAppointmentTypesByPurposeQuery(AppointmentPurpose.Checkup);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPurposeIsInvalid()
    {
        // Arrange
        var query = new GetAppointmentTypesByPurposeQuery((AppointmentPurpose)999);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Purpose)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}

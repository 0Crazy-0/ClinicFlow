using ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypesByCategory;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetAppointmentTypesByCategory;

public class GetAppointmentTypesByCategoryQueryValidatorTests
{
    private readonly GetAppointmentTypesByCategoryQueryValidator _sut;

    public GetAppointmentTypesByCategoryQueryValidatorTests()
    {
        _sut = new GetAppointmentTypesByCategoryQueryValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenCategoryIsValid()
    {
        // Arrange
        var query = new GetAppointmentTypesByCategoryQuery(AppointmentCategory.Checkup);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCategoryIsInvalid()
    {
        // Arrange
        var query = new GetAppointmentTypesByCategoryQuery((AppointmentCategory)999);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }
}

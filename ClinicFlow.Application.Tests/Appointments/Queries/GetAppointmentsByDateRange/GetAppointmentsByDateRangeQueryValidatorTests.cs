using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByDateRange;

public class GetAppointmentsByDateRangeQueryValidatorTests
{
    private readonly GetAppointmentsByDateRangeQueryValidator _sut;

    public GetAppointmentsByDateRangeQueryValidatorTests()
    {
        _sut = new GetAppointmentsByDateRangeQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenStartDateIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentsByDateRangeQuery(default, DateTime.UtcNow);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEndDateIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentsByDateRangeQuery(DateTime.UtcNow, default);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEndDateIsBeforeStartDate()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var query = new GetAppointmentsByDateRangeQuery(now, now.AddDays(-1));

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage(DomainErrors.Validation.InvalidDateRange);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenQueryIsValid()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var query = new GetAppointmentsByDateRangeQuery(now, now.AddDays(7));

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenStartDateEqualsEndDate()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var query = new GetAppointmentsByDateRangeQuery(now, now);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

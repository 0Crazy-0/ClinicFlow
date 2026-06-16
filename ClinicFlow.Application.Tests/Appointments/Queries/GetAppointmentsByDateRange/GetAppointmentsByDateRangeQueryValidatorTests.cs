using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByDateRange;

public class GetAppointmentsByDateRangeQueryValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly GetAppointmentsByDateRangeQueryValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenQueryIsValid()
    {
        // Arrange
        var now = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var query = new GetAppointmentsByDateRangeQuery(now, now.AddDays(7), 1, 10);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenStartDateIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentsByDateRangeQuery(
            default,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime),
            1,
            10
        );

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
        var query = new GetAppointmentsByDateRangeQuery(
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime),
            default,
            1,
            10
        );

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
        var now = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var query = new GetAppointmentsByDateRangeQuery(now, now.AddDays(-1), 1, 10);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage(DomainErrors.Validation.InvalidDateRange);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenStartDateEqualsEndDate()
    {
        // Arrange
        var now = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var query = new GetAppointmentsByDateRangeQuery(now, now, 1, 10);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldHaveError_WhenPageNumberIsLessThanOne(int pageNumber)
    {
        // Arrange
        var now = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var query = new GetAppointmentsByDateRangeQuery(now, now.AddDays(7), pageNumber, 10);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_ShouldHaveError_WhenPageSizeIsOutOfRange(int pageSize)
    {
        // Arrange
        var now = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var query = new GetAppointmentsByDateRangeQuery(now, now.AddDays(7), 1, pageSize);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

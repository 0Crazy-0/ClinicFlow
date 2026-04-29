using ClinicFlow.Application.Schedules.Queries.GetScheduleByDoctorAndDay;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Schedules.Queries.GetScheduleByDoctorAndDay;

public class GetScheduleByDoctorAndDayQueryValidatorTests
{
    private readonly GetScheduleByDoctorAndDayQueryValidator _sut;

    public GetScheduleByDoctorAndDayQueryValidatorTests()
    {
        _sut = new GetScheduleByDoctorAndDayQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var query = new GetScheduleByDoctorAndDayQuery(Guid.Empty, DayOfWeek.Monday);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DoctorId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDayOfWeekIsInvalid()
    {
        // Arrange
        var query = new GetScheduleByDoctorAndDayQuery(Guid.NewGuid(), (DayOfWeek)99);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DayOfWeek)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenQueryIsValid()
    {
        // Arrange
        var query = new GetScheduleByDoctorAndDayQuery(Guid.NewGuid(), DayOfWeek.Wednesday);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DoctorId);
        result.ShouldNotHaveValidationErrorFor(x => x.DayOfWeek);
    }
}

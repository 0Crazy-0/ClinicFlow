using ClinicFlow.Application.Schedules.Queries.GetScheduleById;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Schedules.Queries.GetScheduleById;

public class GetScheduleByIdQueryValidatorTests
{
    private readonly GetScheduleByIdQueryValidator _sut;

    public GetScheduleByIdQueryValidatorTests()
    {
        _sut = new GetScheduleByIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenScheduleIdIsEmpty()
    {
        // Arrange
        var query = new GetScheduleByIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ScheduleId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenScheduleIdIsValid()
    {
        // Arrange
        var query = new GetScheduleByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ScheduleId);
    }
}

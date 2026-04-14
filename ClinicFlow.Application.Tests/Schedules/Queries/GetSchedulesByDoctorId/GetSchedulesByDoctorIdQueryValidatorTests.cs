using ClinicFlow.Application.Schedules.Queries.GetSchedulesByDoctorId;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Schedules.Queries.GetSchedulesByDoctorId;

public class GetSchedulesByDoctorIdQueryValidatorTests
{
    private readonly GetSchedulesByDoctorIdQueryValidator _sut;

    public GetSchedulesByDoctorIdQueryValidatorTests()
    {
        _sut = new GetSchedulesByDoctorIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var query = new GetSchedulesByDoctorIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DoctorId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenDoctorIdIsValid()
    {
        // Arrange
        var query = new GetSchedulesByDoctorIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DoctorId);
    }
}

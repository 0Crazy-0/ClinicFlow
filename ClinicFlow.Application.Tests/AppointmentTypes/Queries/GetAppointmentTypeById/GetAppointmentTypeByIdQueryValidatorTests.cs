using ClinicFlow.Application.AppointmentTypes.Queries.GetAppointmentTypeById;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Queries.GetAppointmentTypeById;

public class GetAppointmentTypeByIdQueryValidatorTests
{
    private readonly GetAppointmentTypeByIdQueryValidator _sut;

    public GetAppointmentTypeByIdQueryValidatorTests()
    {
        _sut = new GetAppointmentTypeByIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenIdIsProvided()
    {
        // Arrange
        var query = new GetAppointmentTypeByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentTypeByIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

using ClinicFlow.Application.Appointments.Queries.GetAppointmentById;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryValidatorTests
{
    private readonly GetAppointmentByIdQueryValidator _sut;

    public GetAppointmentByIdQueryValidatorTests()
    {
        _sut = new GetAppointmentByIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentByIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenAppointmentIdIsValid()
    {
        // Arrange
        var query = new GetAppointmentByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AppointmentId);
    }
}

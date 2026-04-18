using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByPatientId;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByPatientId;

public class GetAppointmentsByPatientIdQueryValidatorTests
{
    private readonly GetAppointmentsByPatientIdQueryValidator _sut;

    public GetAppointmentsByPatientIdQueryValidatorTests()
    {
        _sut = new GetAppointmentsByPatientIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentsByPatientIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenPatientIdIsValid()
    {
        // Arrange
        var query = new GetAppointmentsByPatientIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PatientId);
    }
}

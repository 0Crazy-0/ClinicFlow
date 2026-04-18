using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQueryValidatorTests
{
    private readonly GetAppointmentsByDoctorIdQueryValidator _sut;

    public GetAppointmentsByDoctorIdQueryValidatorTests()
    {
        _sut = new GetAppointmentsByDoctorIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(Guid.Empty, DateTime.UtcNow);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DoctorId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDateIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(Guid.NewGuid(), default);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenQueryIsValid()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(Guid.NewGuid(), DateTime.UtcNow);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DoctorId);
        result.ShouldNotHaveValidationErrorFor(x => x.Date);
    }
}

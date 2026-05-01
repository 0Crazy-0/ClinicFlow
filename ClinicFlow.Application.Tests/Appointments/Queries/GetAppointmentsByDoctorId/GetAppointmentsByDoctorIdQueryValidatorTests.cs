using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQueryValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly GetAppointmentsByDoctorIdQueryValidator _sut;

    public GetAppointmentsByDoctorIdQueryValidatorTests()
    {
        _sut = new GetAppointmentsByDoctorIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(
            Guid.Empty,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DoctorId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDateIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(Guid.NewGuid(), default);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Date)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenQueryIsValid()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DoctorId);
        result.ShouldNotHaveValidationErrorFor(x => x.Date);
    }
}

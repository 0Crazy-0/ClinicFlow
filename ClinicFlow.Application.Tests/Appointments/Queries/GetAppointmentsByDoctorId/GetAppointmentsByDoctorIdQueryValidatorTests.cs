using ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDoctorId;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAppointmentsByDoctorId;

public class GetAppointmentsByDoctorIdQueryValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly GetAppointmentsByDoctorIdQueryValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenQueryIsValid()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime,
            1,
            10
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(
            Guid.Empty,
            _fakeTime.GetUtcNow().UtcDateTime,
            1,
            10
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
        var query = new GetAppointmentsByDoctorIdQuery(Guid.NewGuid(), default, 1, 10);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Date)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldHaveError_WhenPageNumberIsLessThanOne(int pageNumber)
    {
        // Arrange
        var query = new GetAppointmentsByDoctorIdQuery(
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime,
            pageNumber,
            10
        );

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
        var query = new GetAppointmentsByDoctorIdQuery(
            Guid.NewGuid(),
            _fakeTime.GetUtcNow().UtcDateTime,
            1,
            pageSize
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

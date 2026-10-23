using ClinicFlow.Application.Patients.Queries.CheckPatientSsnLast4;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Queries.CheckPatientSsnLast4;

public class CheckPatientSsnLast4QueryValidatorTests
{
    private readonly CheckPatientSsnLast4QueryValidator _sut;

    public CheckPatientSsnLast4QueryValidatorTests()
    {
        _sut = new CheckPatientSsnLast4QueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var query = new CheckPatientSsnLast4Query(Guid.Empty, "1234");

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenQueryIsValid()
    {
        // Arrange
        var query = new CheckPatientSsnLast4Query(Guid.CreateVersion7(), "1234");

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("12a4")]
    public void Validate_ShouldHaveError_WhenLast4IsInvalid(string? last4)
    {
        // Arrange
        var query = new CheckPatientSsnLast4Query(Guid.CreateVersion7(), last4!);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Last4)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

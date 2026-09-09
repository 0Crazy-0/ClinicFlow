using ClinicFlow.Application.MedicalRecords.Queries.GetOwnMedicalRecordsByPatientId;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetOwnMedicalRecordsByPatientId;

public class GetOwnMedicalRecordsByPatientIdQueryValidatorTests
{
    private readonly GetOwnMedicalRecordsByPatientIdQueryValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenQueryIsValid()
    {
        // Arrange
        var query = new GetOwnMedicalRecordsByPatientIdQuery(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            1,
            10
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRequesterUserIdIsEmpty()
    {
        // Arrange
        var query = new GetOwnMedicalRecordsByPatientIdQuery(
            Guid.Empty,
            Guid.CreateVersion7(),
            1,
            10
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RequesterUserId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var query = new GetOwnMedicalRecordsByPatientIdQuery(
            Guid.CreateVersion7(),
            Guid.Empty,
            1,
            10
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldHaveError_WhenPageNumberIsLessThanOne(int pageNumber)
    {
        // Arrange
        var query = new GetOwnMedicalRecordsByPatientIdQuery(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
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
        var query = new GetOwnMedicalRecordsByPatientIdQuery(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
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

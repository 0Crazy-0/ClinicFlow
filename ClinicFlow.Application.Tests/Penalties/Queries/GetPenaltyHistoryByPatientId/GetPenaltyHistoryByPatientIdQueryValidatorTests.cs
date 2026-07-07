using ClinicFlow.Application.Penalties.Queries.GetPenaltyHistoryByPatientId;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Penalties.Queries.GetPenaltyHistoryByPatientId;

public class GetPenaltyHistoryByPatientIdQueryValidatorTests
{
    private readonly GetPenaltyHistoryByPatientIdQueryValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenQueryIsValid()
    {
        // Arrange
        var query = new GetPenaltyHistoryByPatientIdQuery(Guid.NewGuid(), 1, 10);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var query = new GetPenaltyHistoryByPatientIdQuery(Guid.Empty, 1, 10);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldHaveError_WhenPageNumberIsLessThanOne(int pageNumber)
    {
        // Arrange
        var query = new GetPenaltyHistoryByPatientIdQuery(Guid.NewGuid(), pageNumber, 10);

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
        var query = new GetPenaltyHistoryByPatientIdQuery(Guid.NewGuid(), 1, pageSize);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordById;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetMedicalRecordById;

public class GetMedicalRecordByIdQueryValidatorTests
{
    private readonly GetMedicalRecordByIdQueryValidator _sut;

    public GetMedicalRecordByIdQueryValidatorTests()
    {
        _sut = new GetMedicalRecordByIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsEmpty()
    {
        // Arrange
        var query = new GetMedicalRecordByIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenIdIsValid()
    {
        // Arrange
        var query = new GetMedicalRecordByIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}

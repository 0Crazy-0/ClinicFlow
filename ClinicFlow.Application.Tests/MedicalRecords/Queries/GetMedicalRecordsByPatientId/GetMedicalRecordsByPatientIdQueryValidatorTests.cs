using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByPatientId;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetMedicalRecordsByPatientId;

public class GetMedicalRecordsByPatientIdQueryValidatorTests
{
    private readonly GetMedicalRecordsByPatientIdQueryValidator _sut;

    public GetMedicalRecordsByPatientIdQueryValidatorTests()
    {
        _sut = new GetMedicalRecordsByPatientIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var query = new GetMedicalRecordsByPatientIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenPatientIdIsValid()
    {
        // Arrange
        var query = new GetMedicalRecordsByPatientIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PatientId);
    }
}

using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetMedicalRecordsByDoctorId;

public class GetMedicalRecordsByDoctorIdQueryValidatorTests
{
    private readonly GetMedicalRecordsByDoctorIdQueryValidator _sut;

    public GetMedicalRecordsByDoctorIdQueryValidatorTests()
    {
        _sut = new GetMedicalRecordsByDoctorIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var query = new GetMedicalRecordsByDoctorIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DoctorId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenDoctorIdIsValid()
    {
        // Arrange
        var query = new GetMedicalRecordsByDoctorIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DoctorId);
    }
}

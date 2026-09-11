using ClinicFlow.Application.MedicalRecords.Queries.GetFamilyMemberMedicalRecordById;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetFamilyMemberMedicalRecordById;

public class GetFamilyMemberMedicalRecordByIdQueryValidatorTests
{
    private readonly GetFamilyMemberMedicalRecordByIdQueryValidator _sut;

    public GetFamilyMemberMedicalRecordByIdQueryValidatorTests()
    {
        _sut = new GetFamilyMemberMedicalRecordByIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRequesterUserIdIsEmpty()
    {
        // Arrange
        var query = new GetFamilyMemberMedicalRecordByIdQuery(Guid.Empty, Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.RequesterUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenMedicalRecordIdIsEmpty()
    {
        // Arrange
        var query = new GetFamilyMemberMedicalRecordByIdQuery(Guid.CreateVersion7(), Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MedicalRecordId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenIdsAreValid()
    {
        // Arrange
        var query = new GetFamilyMemberMedicalRecordByIdQuery(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RequesterUserId);
        result.ShouldNotHaveValidationErrorFor(x => x.MedicalRecordId);
    }
}

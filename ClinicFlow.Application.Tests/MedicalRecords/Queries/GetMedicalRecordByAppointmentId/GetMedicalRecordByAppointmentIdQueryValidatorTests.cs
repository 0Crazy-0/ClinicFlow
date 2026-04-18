using ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordByAppointmentId;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalRecords.Queries.GetMedicalRecordByAppointmentId;

public class GetMedicalRecordByAppointmentIdQueryValidatorTests
{
    private readonly GetMedicalRecordByAppointmentIdQueryValidator _sut;

    public GetMedicalRecordByAppointmentIdQueryValidatorTests()
    {
        _sut = new GetMedicalRecordByAppointmentIdQueryValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var query = new GetMedicalRecordByAppointmentIdQuery(Guid.Empty);

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenAppointmentIdIsValid()
    {
        // Arrange
        var query = new GetMedicalRecordByAppointmentIdQuery(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AppointmentId);
    }
}

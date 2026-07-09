using ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Doctors.Commands.CreateDoctorProfile;

public class CreateDoctorProfileCommandValidatorTests
{
    private readonly CreateDoctorProfileCommandValidator _sut;

    public CreateDoctorProfileCommandValidatorTests()
    {
        _sut = new CreateDoctorProfileCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "12345",
            Guid.CreateVersion7(),
            "Cardiologist with 10 years of experience",
            10,
            "Cardiology A",
            3
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.Empty,
            "John",
            "Doe",
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenFirstNameIsEmpty(string? firstName)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            firstName!,
            "Doe",
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            3
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameIsTooShort()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "A",
            "Doe",
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            3
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameIsTooLong()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            new string('A', PersonName.MaximumLength + 1),
            "Doe",
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            3
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenLastNameIsEmpty(string? lastName)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            lastName!,
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            3
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLastNameIsTooShort()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "A",
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            3
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLastNameIsTooLong()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            new string('A', PersonName.MaximumLength + 1),
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            3
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLicenseNumberIsEmpty()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LicenseNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLicenseNumberIsTooShort()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "123",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LicenseNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLicenseNumberIsTooLong()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            new string('A', MedicalLicenseNumber.MaximumLength + 1),
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LicenseNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenMedicalSpecialtyIdIsEmpty()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "12345",
            Guid.Empty,
            "Biography",
            10,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MedicalSpecialtyId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(ConsultationRoom.MinimumNumber - 1)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomNumberIsZeroOrNegative(int roomNumber)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            roomNumber,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Theory]
    [InlineData(ConsultationRoom.MaximumNumber + 1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomNumberExceedsMaximum(int roomNumber)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            roomNumber,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenConsultationRoomNameIsEmpty(string? roomName)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            10,
            roomName!,
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(ConsultationRoom.MinimumFloor - 1)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomFloorIsZeroOrNegative(int floor)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            floor
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomFloor)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Theory]
    [InlineData(ConsultationRoom.MaximumFloor + 1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomFloorExceedsMaximum(int floor)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            "12345",
            Guid.CreateVersion7(),
            "Biography",
            10,
            "Room A",
            floor
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomFloor)
            .WithErrorMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }
}

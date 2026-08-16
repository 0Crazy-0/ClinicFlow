using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class PatientTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void CreateProfile_ShouldCreatePatient_WhenValidParameters()
    {
        // Arrange
        var dateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30));
        var bloodType = BloodType.Create("O+");
        var allergies = "Penicillin";
        var chronicConditions = "None";
        var emergencyContact = EmergencyContact.Create("Mom", "555-5555");

        // Act
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            dateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(bloodType, allergies, chronicConditions);
        patient.UpdateEmergencyContact(emergencyContact);

        // Assert
        patient.Should().NotBeNull();
        patient.FullName.Should().Be(PersonName.Create("John Doe"));
        patient.DateOfBirth.Should().Be(dateOfBirth);
        patient.BloodType.Should().Be(bloodType);
        patient.Allergies.Should().Be(allergies);
        patient.ChronicConditions.Should().Be(chronicConditions);
        patient.EmergencyContact.Should().Be(emergencyContact);
    }

    [Fact]
    public void CreateProfile_ShouldThrowException_WhenDateOfBirthIsInTheFuture()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateProfile(
                PersonName.Create("John Doe"),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }

    [Fact]
    public void CreateProfile_ShouldThrowException_WhenFullNameIsNull()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateProfile(
                null!,
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateProfile_ShouldNotThrowException_WhenDateOfBirthIsEqualToReferenceTimeDate()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var dateOfBirth = DateOnly.FromDateTime(referenceTime);

        // Act
        var act = () =>
            Patient.CreateProfile(PersonName.Create("John Doe"), dateOfBirth, referenceTime);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCompleteProfile_ShouldNotThrow_WhenProfileIsComplete()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Mom", "555-5555"));

        // Act & Assert
        patient.Invoking(p => p.EnsureCompleteProfile()).Should().NotThrow();
    }

    [Fact]
    public void EnsureCompleteProfile_ShouldThrowIncompleteProfileException_WhenProfileIsIncomplete()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act && Assert
        patient
            .Invoking(p => p.EnsureCompleteProfile())
            .Should()
            .Throw<IncompleteProfileException>()
            .WithMessage(DomainErrors.Patient.ProfileIncomplete);
    }

    [Fact]
    public void EnsureCompleteProfile_ShouldThrowIncompleteProfileException_WhenBloodTypeIsNullAndEmergencyContactHasValue()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateEmergencyContact(EmergencyContact.Create("Mom", "555-5555"));

        // Act & Assert
        patient
            .Invoking(p => p.EnsureCompleteProfile())
            .Should()
            .Throw<IncompleteProfileException>()
            .WithMessage(DomainErrors.Patient.ProfileIncomplete);
    }

    [Fact]
    public void EnsureCompleteProfile_ShouldThrowIncompleteProfileException_WhenBloodTypeHasValueAndEmergencyContactIsNull()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");

        // Act & Assert
        patient
            .Invoking(p => p.EnsureCompleteProfile())
            .Should()
            .Throw<IncompleteProfileException>()
            .WithMessage(DomainErrors.Patient.ProfileIncomplete);
    }

    [Fact]
    public void UpdateMedicalProfile_ShouldSetEmptyString_WhenNullStringsAreProvided()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var bloodType = BloodType.Create("A-");

        // Act
        patient.UpdateMedicalProfile(bloodType, null!, null!);

        // Assert
        patient.BloodType.Should().Be(bloodType);
        patient.Allergies.Should().Be(string.Empty);
        patient.ChronicConditions.Should().Be(string.Empty);
    }

    [Fact]
    public void GetAge_ShouldReturnCorrectAge()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var yearsAgo = 25;
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(referenceTime.AddYears(-yearsAgo)),
            referenceTime
        );

        // Act & Assert
        patient.GetAge(DateOnly.FromDateTime(referenceTime)).Should().Be(yearsAgo);
    }

    [Fact]
    public void GetAge_ShouldReturnOriginalAgeWithoutAddingOne_WhenBirthdayHasNotOccurredInReferenceYear()
    {
        // Arrange
        var dayBeforeBirthday = new DateTimeOffset(2026, 6, 19, 0, 0, 0, TimeSpan.Zero);

        _fakeTime.SetUtcNow(dayBeforeBirthday);

        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            new DateOnly(2000, 6, 20),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var age = patient.GetAge(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Assert
        age.Should().Be(25);
    }
}

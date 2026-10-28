using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Patients;
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
        var fullName = PersonName.Create("Twilight", "Sparkle", "Doe");
        var dateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30));
        var bloodType = BloodType.Create("O+");
        var allergies = "Penicillin";
        var chronicConditions = "None";
        var emergencyContact = EmergencyContact.Create("Mom", "test", "Mom", "555-5555");
        var homeAddress = Address.Create(
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
        );
        var sexAtBirth = BiologicalSex.Female;

        // Act
        var patient = Patient.CreateProfile(
            fullName,
            dateOfBirth,
            sexAtBirth,
            homeAddress,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(bloodType, allergies, chronicConditions);
        patient.UpdateEmergencyContact(emergencyContact);

        // Assert
        patient.Should().NotBeNull();
        patient.FullName.Should().Be(fullName);
        patient.SearchName.Should().Be(fullName.SearchName);
        patient.DateOfBirth.Should().Be(dateOfBirth);
        patient.SexAtBirth.Should().Be(sexAtBirth);
        patient.BloodGroup.Should().Be(bloodType);
        patient.Allergies.Should().Be(allergies);
        patient.ChronicConditions.Should().Be(chronicConditions);
        patient.Contact.Should().Be(emergencyContact);
        patient.HomeAddress.Should().Be(homeAddress);
    }

    [Theory]
    [InlineData(BiologicalSex.Male)]
    [InlineData(BiologicalSex.Female)]
    public void CreateProfile_ShouldSetBiologicalSex_WhenValidParameters(
        BiologicalSex biologicalSex
    )
    {
        // Arrange
        var fullName = PersonName.Create("John", null, "Doe");
        var dateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30));

        // Act
        var patient = Patient.CreateProfile(
            fullName,
            dateOfBirth,
            biologicalSex,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Assert
        patient.Should().NotBeNull();
        patient.FullName.Should().Be(fullName);
        patient.DateOfBirth.Should().Be(dateOfBirth);
        patient.SexAtBirth.Should().Be(biologicalSex);
    }

    [Fact]
    public void CreateProfile_ShouldThrowException_WhenDateOfBirthIsInTheFuture()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateProfile(
                PersonName.Create("John", null, "Doe"),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
                BiologicalSex.Male,
                Address.Create(
                    "123 Main Street",
                    AddressState.California,
                    "90210",
                    Country.UnitedStates
                ),
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
                BiologicalSex.Male,
                Address.Create(
                    "123 Main Street",
                    AddressState.California,
                    "90210",
                    Country.UnitedStates
                ),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void CreateProfile_ShouldThrowException_WhenAddressIsNull()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateProfile(
                PersonName.Create("John", null, "Doe"),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
                BiologicalSex.Male,
                null!,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void CreateProfile_ShouldThrowDomainValidationException_WhenBiologicalSexIsUndefined()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateProfile(
                PersonName.Create("Crazy", null, "love"),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-67)),
                (BiologicalSex)967,
                Address.Create(
                    "123 Main Street",
                    AddressState.California,
                    "90210",
                    Country.UnitedStates
                ),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void CreateProfile_ShouldThrowDomainValidationException_WhenBiologicalSexIsUnspecified()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateProfile(
                PersonName.Create("John", null, "Doe"),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
                BiologicalSex.Unspecified,
                Address.Create(
                    "123 Main Street",
                    AddressState.California,
                    "90210",
                    Country.UnitedStates
                ),
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
            Patient.CreateProfile(
                PersonName.Create("John", null, "Doe"),
                dateOfBirth,
                BiologicalSex.Male,
                Address.Create(
                    "123 Main Street",
                    AddressState.California,
                    "90210",
                    Country.UnitedStates
                ),
                referenceTime
            );

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCompleteProfile_ShouldNotThrow_WhenProfileIsComplete()
    {
        // Arrange
        var patient = CreateValidPatient();
        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Mom", null, "Mom", "555-5555"));

        // Act & Assert
        patient.Invoking(p => p.EnsureCompleteProfile()).Should().NotThrow();
    }

    [Fact]
    public void EnsureCompleteProfile_ShouldThrowIncompleteProfileException_WhenProfileIsIncomplete()
    {
        // Arrange
        var patient = CreateValidPatient();

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
        var patient = CreateValidPatient();
        patient.UpdateEmergencyContact(EmergencyContact.Create("Mom", null, "Mom", "555-5555"));

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
        var patient = CreateValidPatient();
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
        var patient = CreateValidPatient();
        var bloodType = BloodType.Create("A-");

        // Act
        patient.UpdateMedicalProfile(bloodType, null!, null!);

        // Assert
        patient.BloodGroup.Should().Be(bloodType);
        patient.Allergies.Should().Be(string.Empty);
        patient.ChronicConditions.Should().Be(string.Empty);
    }

    [Fact]
    public void CorrectBiologicalSex_ShouldUpdateValueAndEmitEvent_WhenNewSexDiffers()
    {
        // Arrange
        var patient = CreateValidPatient();

        var correctedByDoctorId = Guid.CreateVersion7();
        var reason = "reason";
        var correctedAt = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        patient.CorrectBiologicalSex(
            BiologicalSex.Female,
            reason,
            correctedByDoctorId,
            correctedAt
        );

        // Assert
        patient.SexAtBirth.Should().Be(BiologicalSex.Female);
        patient.DomainEvents.OfType<PatientBiologicalSexCorrectedEvent>().Should().ContainSingle();

        var evt = patient.DomainEvents.OfType<PatientBiologicalSexCorrectedEvent>().Single();
        evt.PatientId.Should().Be(patient.Id);
        evt.PreviousSex.Should().Be(BiologicalSex.Male);
        evt.CorrectedSex.Should().Be(BiologicalSex.Female);
        evt.CorrectedByDoctorId.Should().Be(correctedByDoctorId);
        evt.Reason.Should().Be(reason);
        evt.CorrectedAt.Should().Be(correctedAt);
    }

    [Fact]
    public void CorrectBiologicalSex_ShouldThrowBusinessRuleValidationException_WhenSameAsCurrent()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("John", null, "Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            BiologicalSex.Female,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () =>
            patient.CorrectBiologicalSex(
                BiologicalSex.Female,
                "No change",
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Patient.BiologicalSexUnchanged);

        patient.DomainEvents.OfType<PatientBiologicalSexCorrectedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void CorrectBiologicalSex_ShouldThrowDomainValidationException_WhenCorrectedSexIsUndefined()
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act
        var act = () =>
            patient.CorrectBiologicalSex(
                (BiologicalSex)967,
                "reason",
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void CorrectBiologicalSex_ShouldThrowDomainValidationException_WhenCorrectedSexIsUnspecified()
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act
        var act = () =>
            patient.CorrectBiologicalSex(
                BiologicalSex.Unspecified,
                "reason",
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CorrectBiologicalSex_ShouldThrowDomainValidationException_WhenReasonIsEmpty(
        string? reason
    )
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act
        var act = () =>
            patient.CorrectBiologicalSex(
                BiologicalSex.Female,
                reason!,
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CorrectBiologicalSex_ShouldThrowDomainValidationException_WhenCorrectedByDoctorIdIsEmpty()
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act
        var act = () =>
            patient.CorrectBiologicalSex(
                BiologicalSex.Female,
                "reason",
                Guid.Empty,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CorrectBiologicalSex_ShouldThrowDomainValidationException_WhenReasonIsTooLong()
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act
        var act = () =>
            patient.CorrectBiologicalSex(
                BiologicalSex.Female,
                new string('A', Patient.MaxCorrectionReasonLength + 1),
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void CorrectBiologicalSex_ShouldNotThrowException_WhenReasonIsAtMaxLength()
    {
        // Arrange
        var patient = CreateValidPatient();
        var reason = new string('A', Patient.MaxCorrectionReasonLength);

        // Act
        var act = () =>
            patient.CorrectBiologicalSex(
                BiologicalSex.Female,
                reason,
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should().NotThrow();
        patient.SexAtBirth.Should().Be(BiologicalSex.Female);
    }

    [Fact]
    public void GetAge_ShouldReturnCorrectAge()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var yearsAgo = 25;
        var patient = Patient.CreateProfile(
            PersonName.Create("John", null, "Doe"),
            DateOnly.FromDateTime(referenceTime.AddYears(-yearsAgo)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
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
            PersonName.Create("John", null, "Doe"),
            new DateOnly(2000, 6, 20),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var age = patient.GetAge(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Assert
        age.Should().Be(25);
    }

    [Fact]
    public void AssignSsnLast4Hash_ShouldSetHash_WhenValidHash()
    {
        // Arrange
        var patient = CreateValidPatient();
        var hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

        // Act
        patient.AssignSsnLast4Hash(hash);

        // Assert
        patient.SsnLast4Hash.Should().Be(hash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AssignSsnLast4Hash_ShouldThrowDomainValidationException_WhenHashIsMissing(
        string? hash
    )
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act && Assert
        var act = () => patient.AssignSsnLast4Hash(hash!);

        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    private Patient CreateValidPatient() =>
        Patient.CreateProfile(
            PersonName.Create("John", null, "Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );
}

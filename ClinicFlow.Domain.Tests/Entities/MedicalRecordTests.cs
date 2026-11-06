using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Tests.Entities;

public class MedicalRecordTests
{
    [Fact]
    public void Create_ShouldCreateMedicalRecord_WhenValidDataProvided()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var chiefComplaint = "Headache and fever";

        // Act
        var record = MedicalRecord.Create(
            patientId,
            doctorId,
            appointmentId,
            chiefComplaint,
            null,
            null
        );

        // Assert
        record.Should().NotBeNull();
        record.PatientId.Should().Be(patientId);
        record.DoctorId.Should().Be(doctorId);
        record.AppointmentId.Should().Be(appointmentId);
        record.ChiefComplaint.Should().Be(chiefComplaint);
        record.ClinicalDetails.Should().BeEmpty();
        record.ProtectedCareCategory.Should().BeNull();
        record.GuardianInitiatedTreatment.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldAssignProtectedCategory_WhenValidProtectedCategoryProvided()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var chiefComplaint = "Headache and fever";

        // Act
        var record = MedicalRecord.Create(
            patientId,
            doctorId,
            appointmentId,
            chiefComplaint,
            ProtectedCategory.MentalHealthCounseling,
            null
        );

        // Assert
        record.PatientId.Should().Be(patientId);
        record.DoctorId.Should().Be(doctorId);
        record.AppointmentId.Should().Be(appointmentId);
        record.ChiefComplaint.Should().Be(chiefComplaint);
        record.ClinicalDetails.Should().BeEmpty();
        record.ProtectedCareCategory.Should().Be(ProtectedCategory.MentalHealthCounseling);
        record.GuardianInitiatedTreatment.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_ShouldAssignGuardianInitiatedTreatment_WhenSubstanceAbuseTreatmentProvided(
        bool guardianInitiatedTreatment
    )
    {
        // Arrange & Act
        var record = MedicalRecord.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Headache and fever",
            ProtectedCategory.SubstanceAbuseTreatment,
            guardianInitiatedTreatment
        );

        // Assert
        record.ProtectedCareCategory.Should().Be(ProtectedCategory.SubstanceAbuseTreatment);
        record.GuardianInitiatedTreatment.Should().Be(guardianInitiatedTreatment);
        record.GuardianInvolvementDeemedAppropriate.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            MedicalRecord.Create(
                Guid.Empty,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Headache and fever",
                null,
                null
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDoctorIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            MedicalRecord.Create(
                Guid.CreateVersion7(),
                Guid.Empty,
                Guid.CreateVersion7(),
                "Headache and fever",
                null,
                null
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenAppointmentIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            MedicalRecord.Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.Empty,
                "chiefComplaint",
                null,
                null
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
    public void Create_ShouldThrowException_WhenChiefComplaintIsEmpty(string? chiefComplaint)
    {
        // Arrange & Act
        var act = () =>
            MedicalRecord.Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                chiefComplaint!,
                null,
                null
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenProtectedCategoryIsNotDefined()
    {
        // Arrange & Act
        var act = () =>
            MedicalRecord.Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Headache and fever",
                (ProtectedCategory)999,
                null
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenSubstanceAbuseTreatmentLacksGuardianInitiatedTreatment()
    {
        // Arrange & Act
        var act = () =>
            MedicalRecord.Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "Headache and fever",
                ProtectedCategory.SubstanceAbuseTreatment,
                guardianInitiatedTreatment: null
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecord.GuardianInitiatedTreatmentRequired);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_ShouldThrowException_WhenGuardianInitiatedTreatmentProvidedForNonSubstanceAbuseCategory(
        bool guardianInitiatedTreatment
    )
    {
        // Arrange & Act
        var act = () =>
            MedicalRecord.Create(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "chiefComplaint",
                ProtectedCategory.MentalHealthCounseling,
                guardianInitiatedTreatment
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecord.GuardianInitiatedTreatmentNotApplicable);
    }

    [Fact]
    public void AddClinicalDetail_ShouldAddDetail_WhenValid()
    {
        // Arrange
        var record = CreateValidMedicalRecord();
        var detail = DynamicClinicalDetail.Create("VITALS", "{}");

        // Act
        record.AddClinicalDetail(detail);

        // Assert
        record.ClinicalDetails.Should().ContainSingle();
        record.ClinicalDetails.First().Should().Be(detail);
    }

    [Fact]
    public void AddClinicalDetail_ShouldThrowException_WhenDetailIsNull()
    {
        // Arrange
        var record = CreateValidMedicalRecord();

        // Act
        var act = () => record.AddClinicalDetail(null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void AddClinicalDetail_ShouldThrowException_WhenDuplicateDetailTypeAdded()
    {
        // Arrange
        var record = CreateValidMedicalRecord();
        var detail1 = DynamicClinicalDetail.Create("VITALS", "{}");
        var detail2 = DynamicClinicalDetail.Create("VITALS", "{}");

        record.AddClinicalDetail(detail1);

        // Act
        var act = () => record.AddClinicalDetail(detail2);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalEncounter.DetailAlreadyExists);
    }

    [Theory]
    [InlineData(ProtectedCategory.MentalHealthCounseling, true)]
    [InlineData(ProtectedCategory.ResidentialShelter, false)]
    public void SetGuardianInvolvementDetermination_ShouldAssignValue_WhenCategoryIsEligibleForGuardianInvolvement(
        ProtectedCategory protectedCareCategory,
        bool guardianInvolvementDeemedAppropriate
    )
    {
        // Arrange
        var record = CreateValidMedicalRecord(protectedCareCategory);

        // Act
        record.SetGuardianInvolvementDetermination(guardianInvolvementDeemedAppropriate);

        // Assert
        record
            .GuardianInvolvementDeemedAppropriate.Should()
            .Be(guardianInvolvementDeemedAppropriate);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(ProtectedCategory.SubstanceAbuseTreatment, false)]
    [InlineData(ProtectedCategory.PregnancyPrevention, null)]
    public void SetGuardianInvolvementDetermination_ShouldThrowException_WhenCategoryIsNotEligibleForGuardianInvolvement(
        ProtectedCategory? protectedCareCategory,
        bool? guardianInitiatedTreatment
    )
    {
        // Arrange
        var record = CreateValidMedicalRecord(protectedCareCategory, guardianInitiatedTreatment);

        // Act
        var act = () => record.SetGuardianInvolvementDetermination(true);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecord.GuardianInvolvementFlagNotApplicable);
    }

    private static MedicalRecord CreateValidMedicalRecord(
        ProtectedCategory? protectedCareCategory = null,
        bool? guardianInitiatedTreatment = null
    ) =>
        MedicalRecord.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "General checkup",
            protectedCareCategory,
            guardianInitiatedTreatment
        );
}

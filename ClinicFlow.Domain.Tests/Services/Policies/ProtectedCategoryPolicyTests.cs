using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Policies;

namespace ClinicFlow.Domain.Tests.Services.Policies;

public class ProtectedCategoryPolicyTests
{
    [Theory]
    [InlineData(ProtectedCategory.MentalHealthCounseling, 12)]
    [InlineData(ProtectedCategory.ResidentialShelter, 12)]
    [InlineData(ProtectedCategory.PregnancyPrevention, 0)]
    [InlineData(ProtectedCategory.CommunicableDisease, 12)]
    [InlineData(ProtectedCategory.STIPreventionOrTreatment, 12)]
    [InlineData(ProtectedCategory.SexualAssaultCare, 0)]
    [InlineData(ProtectedCategory.RapeCare, 12)]
    [InlineData(ProtectedCategory.SubstanceAbuseTreatment, 12)]
    [InlineData(ProtectedCategory.BuprenorphineOpioidTreatment, 16)]
    [InlineData(ProtectedCategory.NarcoticTreatmentProgram, 16)]
    [InlineData(ProtectedCategory.IntimatePartnerViolenceCare, 12)]
    public void MinimumConsentAge_ShouldReturnStatutoryAge_ForEachProtectedCategory(
        ProtectedCategory category,
        int expectedAge
    )
    {
        // Act & Assert
        ProtectedCategoryPolicy.MinimumConsentAge(category).Should().Be(expectedAge);
    }

    [Theory]
    [InlineData(999)]
    [InlineData(-1)]
    public void MinimumConsentAge_ShouldThrowDomainValidationException_WhenCategoryIsUndefined(
        int undefinedCategoryValue
    )
    {
        // Arrange
        var undefinedCategory = (ProtectedCategory)undefinedCategoryValue;

        // Act
        var act = () => ProtectedCategoryPolicy.MinimumConsentAge(undefinedCategory);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenCategoryIsNull()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: null,
                patientAge: 10,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenPatientAgeIsOneBelowMinimumConsentAge()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.MentalHealthCounseling,
                patientAge: 11,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeFalse();

        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.BuprenorphineOpioidTreatment,
                patientAge: 15,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenPatientAgeEqualsMinimumConsentAge()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.MentalHealthCounseling,
                patientAge: 12,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeTrue();

        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.BuprenorphineOpioidTreatment,
                patientAge: 16,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenPatientAgeIsAboveMinimumConsentAge()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.MentalHealthCounseling,
                patientAge: 13,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeTrue();

        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.BuprenorphineOpioidTreatment,
                patientAge: 17,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenAgeAbsoluteCategoryIsEvaluatedForANewborn()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.PregnancyPrevention,
                patientAge: 0,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeTrue();

        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.SexualAssaultCare,
                patientAge: 0,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenAgeAbsoluteCategoryIsEvaluatedForAnAdult()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.PregnancyPrevention,
                patientAge: 40,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenPatientAgeIsOneBelowAgeOfMajority()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.MentalHealthCounseling,
                patientAge: 17,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenPatientReachesAgeOfMajority()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.MentalHealthCounseling,
                patientAge: 18,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeFalse();

        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.PregnancyPrevention,
                patientAge: 18,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenSubstanceAbuseTreatmentWasGuardianInitiated()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.SubstanceAbuseTreatment,
                patientAge: 15,
                guardianInitiatedTreatment: true,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenSubstanceAbuseTreatmentWasNotGuardianInitiated()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.SubstanceAbuseTreatment,
                patientAge: 15,
                guardianInitiatedTreatment: false,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenSubstanceAbuseTreatmentHasNoGuardianInitiationData()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.SubstanceAbuseTreatment,
                patientAge: 15,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: null
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenMentalHealthCounselingGuardianInvolvementWasDeemedAppropriate()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.MentalHealthCounseling,
                patientAge: 15,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: true
            )
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenMentalHealthCounselingGuardianInvolvementWasNotDeemedAppropriate()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.MentalHealthCounseling,
                patientAge: 15,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: false
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenResidentialShelterGuardianInvolvementWasDeemedAppropriate()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.ResidentialShelter,
                patientAge: 15,
                guardianInitiatedTreatment: null,
                guardianInvolvementDeemedAppropriate: true
            )
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenGuardianFlagsAreIrrelevantForOtherCategories()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(
                category: ProtectedCategory.PregnancyPrevention,
                patientAge: 10,
                guardianInitiatedTreatment: true,
                guardianInvolvementDeemedAppropriate: true
            )
            .Should()
            .BeTrue();
    }

    [Fact]
    public void GetProtectedCategoriesFor_ShouldReturnOnlyAgeAbsoluteCategories_WhenPatientIsANewborn()
    {
        // Act
        var protectedCategories = ProtectedCategoryPolicy.GetProtectedCategoriesFor(0);

        // Assert
        protectedCategories
            .Should()
            .BeEquivalentTo([
                ProtectedCategory.PregnancyPrevention,
                ProtectedCategory.SexualAssaultCare,
            ]);
    }

    [Fact]
    public void GetProtectedCategoriesFor_ShouldReturnOnlyAgeAbsoluteCategories_WhenPatientIsElevenYearsOld()
    {
        // Act
        var protectedCategories = ProtectedCategoryPolicy.GetProtectedCategoriesFor(11);

        // Assert
        protectedCategories
            .Should()
            .BeEquivalentTo([
                ProtectedCategory.PregnancyPrevention,
                ProtectedCategory.SexualAssaultCare,
            ]);
    }

    [Fact]
    public void GetProtectedCategoriesFor_ShouldReturnCategoriesWithMinimumAgeOfTwelveOrLower_WhenPatientIsTwelveYearsOld()
    {
        // Act
        var protectedCategories = ProtectedCategoryPolicy.GetProtectedCategoriesFor(12);

        // Assert
        protectedCategories
            .Should()
            .BeEquivalentTo([
                ProtectedCategory.MentalHealthCounseling,
                ProtectedCategory.ResidentialShelter,
                ProtectedCategory.PregnancyPrevention,
                ProtectedCategory.CommunicableDisease,
                ProtectedCategory.STIPreventionOrTreatment,
                ProtectedCategory.SexualAssaultCare,
                ProtectedCategory.RapeCare,
                ProtectedCategory.SubstanceAbuseTreatment,
                ProtectedCategory.IntimatePartnerViolenceCare,
            ]);
    }

    [Fact]
    public void GetProtectedCategoriesFor_ShouldNotReturnSixteenAgeCategories_WhenPatientIsFifteenYearsOld()
    {
        // Act
        var protectedCategories = ProtectedCategoryPolicy.GetProtectedCategoriesFor(15);

        // Assert
        protectedCategories
            .Should()
            .HaveCount(9)
            .And.NotContain(ProtectedCategory.BuprenorphineOpioidTreatment)
            .And.NotContain(ProtectedCategory.NarcoticTreatmentProgram);
    }

    [Fact]
    public void GetProtectedCategoriesFor_ShouldReturnEveryCategory_WhenPatientIsSixteenYearsOld()
    {
        // Act
        var protectedCategories = ProtectedCategoryPolicy.GetProtectedCategoriesFor(16);

        // Assert
        protectedCategories.Should().BeEquivalentTo(Enum.GetValues<ProtectedCategory>());
    }

    [Fact]
    public void GetProtectedCategoriesFor_ShouldReturnEveryCategory_WhenPatientIsOneBelowAgeOfMajority()
    {
        // Act
        var protectedCategories = ProtectedCategoryPolicy.GetProtectedCategoriesFor(17);

        // Assert
        protectedCategories.Should().BeEquivalentTo(Enum.GetValues<ProtectedCategory>());
    }

    [Fact]
    public void GetProtectedCategoriesFor_ShouldReturnEmptyList_WhenPatientReachesAgeOfMajority()
    {
        // Act
        var protectedCategories = ProtectedCategoryPolicy.GetProtectedCategoriesFor(18);

        // Assert
        protectedCategories.Should().BeEmpty();
    }

    [Fact]
    public void IsVisibleToFamilyMember_ShouldReturnTrue_WhenRecordHasNoProtectedCategory()
    {
        // Arrange
        var record = CreateMedicalRecord(
            protectedCareCategory: null,
            guardianInitiatedTreatment: null
        );

        // Act & Assert
        IsVisibleToFamilyMember(record, [ProtectedCategory.MentalHealthCounseling])
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsVisibleToFamilyMember_ShouldReturnFalse_WhenRecordCategoryIsExcludedAndNoGuardianFlags()
    {
        // Arrange
        var record = CreateMedicalRecord(
            protectedCareCategory: ProtectedCategory.MentalHealthCounseling,
            guardianInitiatedTreatment: null
        );

        // Act & Assert
        IsVisibleToFamilyMember(record, [ProtectedCategory.MentalHealthCounseling])
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsVisibleToFamilyMember_ShouldReturnTrue_WhenRecordCategoryIsNotExcluded()
    {
        // Arrange
        var record = CreateMedicalRecord(
            protectedCareCategory: ProtectedCategory.PregnancyPrevention,
            guardianInitiatedTreatment: null
        );

        // Act & Assert
        IsVisibleToFamilyMember(record, [ProtectedCategory.MentalHealthCounseling])
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsVisibleToFamilyMember_ShouldReturnTrue_WhenExcludedSubstanceAbuseTreatmentWasGuardianInitiated()
    {
        // Arrange
        var record = CreateMedicalRecord(
            protectedCareCategory: ProtectedCategory.SubstanceAbuseTreatment,
            guardianInitiatedTreatment: true
        );

        // Act & Assert
        IsVisibleToFamilyMember(record, [ProtectedCategory.SubstanceAbuseTreatment])
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsVisibleToFamilyMember_ShouldReturnFalse_WhenExcludedSubstanceAbuseTreatmentWasNotGuardianInitiated()
    {
        // Arrange
        var record = CreateMedicalRecord(
            protectedCareCategory: ProtectedCategory.SubstanceAbuseTreatment,
            guardianInitiatedTreatment: false
        );

        // Act & Assert
        IsVisibleToFamilyMember(record, [ProtectedCategory.SubstanceAbuseTreatment])
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsVisibleToFamilyMember_ShouldReturnTrue_WhenExcludedMentalHealthCounselingHadGuardianInvolvementDeemedAppropriate()
    {
        // Arrange
        var record = CreateMedicalRecord(
            protectedCareCategory: ProtectedCategory.MentalHealthCounseling,
            guardianInitiatedTreatment: null
        );

        record.SetGuardianInvolvementDetermination(true);

        // Act & Assert
        IsVisibleToFamilyMember(record, [ProtectedCategory.MentalHealthCounseling])
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsVisibleToFamilyMember_ShouldReturnFalse_WhenExcludedMentalHealthCounselingHadGuardianInvolvementDeemedInappropriate()
    {
        // Arrange
        var record = CreateMedicalRecord(
            protectedCareCategory: ProtectedCategory.MentalHealthCounseling,
            guardianInitiatedTreatment: null
        );
        record.SetGuardianInvolvementDetermination(false);

        // Act & Assert
        IsVisibleToFamilyMember(record, [ProtectedCategory.MentalHealthCounseling])
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsVisibleToFamilyMember_ShouldReturnFalse_WhenExcludedResidentialShelterHadNoDetermination()
    {
        // Arrange
        var record = CreateMedicalRecord(
            protectedCareCategory: ProtectedCategory.ResidentialShelter,
            guardianInitiatedTreatment: null
        );

        // Act & Assert
        IsVisibleToFamilyMember(record, [ProtectedCategory.ResidentialShelter]).Should().BeFalse();
    }

    [Fact]
    public void IsVisibleToFamilyMember_ShouldReturnTrue_WhenExcludedResidentialShelterHadGuardianInvolvementDeemedAppropriate()
    {
        // Arrange
        var record = CreateMedicalRecord(
            protectedCareCategory: ProtectedCategory.ResidentialShelter,
            guardianInitiatedTreatment: null
        );
        record.SetGuardianInvolvementDetermination(true);

        // Act & Assert
        IsVisibleToFamilyMember(record, [ProtectedCategory.ResidentialShelter]).Should().BeTrue();
    }

    private static bool IsVisibleToFamilyMember(
        MedicalRecord record,
        IReadOnlyCollection<ProtectedCategory> excludedCategories
    ) => ProtectedCategoryPolicy.IsVisibleToFamilyMember(excludedCategories).Compile()(record);

    private static MedicalRecord CreateMedicalRecord(
        ProtectedCategory? protectedCareCategory,
        bool? guardianInitiatedTreatment
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

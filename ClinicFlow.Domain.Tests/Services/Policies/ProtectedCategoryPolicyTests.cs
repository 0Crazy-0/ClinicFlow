using AwesomeAssertions;
using ClinicFlow.Domain.Enums;
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
    public void MinimumConsentAge_ShouldThrowArgumentOutOfRangeException_WhenCategoryIsUndefined(
        int undefinedCategoryValue
    )
    {
        // Arrange
        var undefinedCategory = (ProtectedCategory)undefinedCategoryValue;

        // Act
        var act = () => ProtectedCategoryPolicy.MinimumConsentAge(undefinedCategory);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenCategoryIsNull()
    {
        // Act & Assert
        ProtectedCategoryPolicy.IsProtectedForPatient(null, 10).Should().BeFalse();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenPatientAgeIsOneBelowMinimumConsentAge()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.MentalHealthCounseling, 11)
            .Should()
            .BeFalse();
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.BuprenorphineOpioidTreatment, 15)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenPatientAgeEqualsMinimumConsentAge()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.MentalHealthCounseling, 12)
            .Should()
            .BeTrue();
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.BuprenorphineOpioidTreatment, 16)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenPatientAgeIsAboveMinimumConsentAge()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.MentalHealthCounseling, 13)
            .Should()
            .BeTrue();
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.BuprenorphineOpioidTreatment, 17)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenAgeAbsoluteCategoryIsEvaluatedForANewborn()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.PregnancyPrevention, 0)
            .Should()
            .BeTrue();
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.SexualAssaultCare, 0)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenAgeAbsoluteCategoryIsEvaluatedForAnAdult()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.PregnancyPrevention, 40)
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnTrue_WhenPatientAgeIsOneBelowAgeOfMajority()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.MentalHealthCounseling, 17)
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsProtectedForPatient_ShouldReturnFalse_WhenPatientReachesAgeOfMajority()
    {
        // Act & Assert
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.MentalHealthCounseling, 18)
            .Should()
            .BeFalse();
        ProtectedCategoryPolicy
            .IsProtectedForPatient(ProtectedCategory.PregnancyPrevention, 18)
            .Should()
            .BeFalse();
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
}

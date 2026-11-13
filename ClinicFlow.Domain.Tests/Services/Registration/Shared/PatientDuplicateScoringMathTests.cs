using AwesomeAssertions;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Services.Registration.Shared;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.Services.Registration.Shared;

public class PatientDuplicateScoringMathTests
{
    [Fact]
    public void ComputeAddressWeight_ShouldReturnFullWeight_WhenAddressesAreIdentical()
    {
        // Arrange
        var formAddress = CreateTexasAddress("Maple Street", "75001");
        var candidateAddress = CreateTexasAddress("Maple Street", "75001");

        // Act
        var weight = PatientDuplicateScoringMath.ComputeAddressWeight(
            formAddress,
            candidateAddress
        );

        // Assert
        weight.Should().Be(5.0);
    }

    [Fact]
    public void ComputeAddressWeight_ShouldReturnZero_WhenOnlyStateMatches()
    {
        // Arrange
        var formAddress = CreateTexasAddress("Maple Street", "75001");
        var candidateAddress = CreateTexasAddress("Maple Street", "75002");

        // Act
        var weight = PatientDuplicateScoringMath.ComputeAddressWeight(
            formAddress,
            candidateAddress
        );

        // Assert
        weight.Should().Be(0.0);
    }

    [Fact]
    public void ComputeAddressWeight_ShouldReturnZero_WhenStateDiffers()
    {
        // Arrange
        var formAddress = CreateTexasAddress("Maple Street", "75001"); // states is: AddressState.Texas
        var candidateAddress = Address.Create(
            "Maple Street",
            AddressState.Florida,
            "75001",
            Country.UnitedStates
        );

        // Act
        var weight = PatientDuplicateScoringMath.ComputeAddressWeight(
            formAddress,
            candidateAddress
        );

        // Assert
        weight.Should().Be(0.0);
    }

    [Fact]
    public void ComputeAddressWeight_ShouldReturnHalfWeight_WhenLinesShareHalfTheirCharacters()
    {
        // Arrange
        // canonicalized "oak" and "oak st" have edit distance 3 of a maximum 6,
        // so the similarity is 0.5. The similarity must be multiplied by the
        // address weight, giving 5 * 0.5 = 2.5.
        var formAddress = CreateTexasAddress("Oak", "75001");
        var candidateAddress = CreateTexasAddress("Oak St", "75001");

        // Act
        var weight = PatientDuplicateScoringMath.ComputeAddressWeight(
            formAddress,
            candidateAddress
        );

        // Assert
        weight.Should().Be(2.5);
    }

    [Fact]
    public void Similarity_ShouldReturnOne_WhenBothValuesAreEmpty()
    {
        // Arrange & Act
        var similarity = PatientDuplicateScoringMath.Similarity("", "");

        // Assert
        similarity.Should().Be(1.0);
    }

    [Fact]
    public void Similarity_ShouldReturnZero_WhenOnlyLeftValueIsEmpty()
    {
        // Arrange & Act
        var similarity = PatientDuplicateScoringMath.Similarity("", "oak");

        // Assert
        similarity.Should().Be(0.0);
    }

    [Fact]
    public void Similarity_ShouldReturnZero_WhenOnlyRightValueIsEmpty()
    {
        // Arrange & Act
        var similarity = PatientDuplicateScoringMath.Similarity("oak", "");

        // Assert
        similarity.Should().Be(0.0);
    }

    [Fact]
    public void Similarity_ShouldReturnZero_WhenValuesAreCompletelyDifferent()
    {
        // Arrange & Act
        var similarity = PatientDuplicateScoringMath.Similarity("a", "b");

        // Assert
        similarity.Should().Be(0.0);
    }

    [Fact]
    public void Similarity_ShouldReturnHalf_WhenSecondValueIsTwiceAsLong()
    {
        // Arrange & Act
        // "ab" and "abcd" have edit distance 2 of a maximum 4, so the similarity
        // is 1 - 2/4 = 0.5. The distance must be divided by the longer length,
        // not multiplied by it nor divided by the shorter one.
        var similarity = PatientDuplicateScoringMath.Similarity("ab", "abcd");

        // Assert
        similarity.Should().Be(0.5);
    }

    private static Address CreateTexasAddress(string line, string postalCode) =>
        Address.Create(line, AddressState.Texas, postalCode, Country.UnitedStates);
}

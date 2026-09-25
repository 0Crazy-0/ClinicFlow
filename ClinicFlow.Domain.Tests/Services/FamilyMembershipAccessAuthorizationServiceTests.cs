using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class FamilyMembershipAccessAuthorizationServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void CanManageFamilyMembership_ShouldReturnFalse_WhenRequesterModifiesOwnMembership()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var context = CreateContext(
            yearsOld: 30,
            patientsSelf: true,
            requesterUserId: userId,
            targetUserId: userId
        );

        // Act
        var result = FamilyMembershipAccessAuthorizationService.CanManageFamilyMembership(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanManageFamilyMembership_ShouldReturnFalse_WhenPatientIsMinorAndRequesterIsPatientsSelf()
    {
        // Arrange
        // 15 years old: old enough to hold a self membership (MinimumSelfAge is 12)
        // but still a minor, so management stays disabled.
        var context = CreateContext(
            yearsOld: 15,
            patientsSelf: true,
            requesterUserId: Guid.CreateVersion7(),
            targetUserId: Guid.CreateVersion7()
        );

        // Act
        var result = FamilyMembershipAccessAuthorizationService.CanManageFamilyMembership(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanManageFamilyMembership_ShouldReturnFalse_WhenPatientIsMinorAndRequesterIsNotPatientsSelf()
    {
        // Arrange
        var context = CreateContext(
            yearsOld: 10,
            patientsSelf: false,
            requesterUserId: Guid.CreateVersion7(),
            targetUserId: Guid.CreateVersion7()
        );

        // Act
        var result = FamilyMembershipAccessAuthorizationService.CanManageFamilyMembership(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanManageFamilyMembership_ShouldReturnTrue_WhenPatientIsAdultAndRequesterIsPatientsSelf()
    {
        // Arrange
        var context = CreateContext(
            yearsOld: 67,
            patientsSelf: true,
            requesterUserId: Guid.CreateVersion7(),
            targetUserId: Guid.CreateVersion7()
        );

        // Act
        var result = FamilyMembershipAccessAuthorizationService.CanManageFamilyMembership(context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanManageFamilyMembership_ShouldReturnFalse_WhenPatientIsAdultAndRequesterIsNotPatientsSelf()
    {
        // Arrange
        var context = CreateContext(
            yearsOld: 30,
            patientsSelf: false,
            requesterUserId: Guid.CreateVersion7(),
            targetUserId: Guid.CreateVersion7()
        );

        // Act
        var result = FamilyMembershipAccessAuthorizationService.CanManageFamilyMembership(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanManageFamilyMembership_ShouldTreatPatientAsAdult_WhenPatientIsExactlyEighteenYearsOld()
    {
        // Arrange
        var context = CreateContext(
            yearsOld: 18,
            patientsSelf: true,
            requesterUserId: Guid.CreateVersion7(),
            targetUserId: Guid.CreateVersion7()
        );

        // Act
        var result = FamilyMembershipAccessAuthorizationService.CanManageFamilyMembership(context);

        // Assert
        result.Should().BeTrue();
    }

    private FamilyMembershipManagementAuthorizationContext CreateContext(
        int yearsOld,
        bool patientsSelf,
        Guid requesterUserId,
        Guid targetUserId
    )
    {
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(referenceTime.AddYears(-yearsOld)),
            referenceTime
        );

        return new FamilyMembershipManagementAuthorizationContext
        {
            Patient = patient,
            ReferenceTime = referenceTime,
            RequesterUserId = requesterUserId,
            TargetUserId = targetUserId,
            RequesterIsPatientsSelf = patientsSelf,
        };
    }
}

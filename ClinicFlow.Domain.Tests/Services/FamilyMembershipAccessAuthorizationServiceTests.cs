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
    public void CanChangeAccessLevel_ShouldReturnTrue_WhenPatientIsMinorAndRequesterIsAuthorized()
    {
        // Arrange
        var context = CreateContext(
            yearsOld: 10,
            selfMembership: true,
            patientsSelf: false,
            activeMembership: true
        );

        // Act
        var result = FamilyMembershipAccessAuthorizationService.CanChangeAccessLevel(context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanChangeAccessLevel_ShouldReturnFalse_WhenPatientIsMinorAndRequesterIsNotAuthorized()
    {
        // Arrange
        var context = CreateContext(
            yearsOld: 10,
            selfMembership: false,
            patientsSelf: false,
            activeMembership: true
        );

        // Act
        var result = FamilyMembershipAccessAuthorizationService.CanChangeAccessLevel(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanChangeAccessLevel_ShouldReturnTrue_WhenPatientIsAdultAndRequesterIsPatientsSelf()
    {
        // Arrange
        var context = CreateContext(
            yearsOld: 30,
            selfMembership: true,
            patientsSelf: true,
            activeMembership: true
        );

        // Act
        var result = FamilyMembershipAccessAuthorizationService.CanChangeAccessLevel(context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanChangeAccessLevel_ShouldReturnFalse_WhenPatientIsAdultAndRequesterIsNotPatientsSelf()
    {
        // Arrange
        var context = CreateContext(
            yearsOld: 30,
            selfMembership: true,
            patientsSelf: false,
            activeMembership: true
        );

        // Act
        var result = FamilyMembershipAccessAuthorizationService.CanChangeAccessLevel(context);

        // Assert
        result.Should().BeFalse();
    }

    private AccessLevelChangeAuthorizationContext CreateContext(
        int yearsOld,
        bool selfMembership,
        bool patientsSelf,
        bool activeMembership
    )
    {
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(referenceTime.AddYears(-yearsOld)),
            referenceTime
        );

        return new AccessLevelChangeAuthorizationContext
        {
            Patient = patient,
            ReferenceTime = referenceTime,
            RequesterHasSelfMembership = selfMembership,
            RequesterIsPatientsSelf = patientsSelf,
            RequesterHasActiveMembershipWithPatient = activeMembership,
        };
    }
}

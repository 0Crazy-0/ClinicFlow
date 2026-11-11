using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.FamilyMemberships;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Registration;

public class MinorFamilyLinkServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Link_ShouldThrowArgumentNullException_WhenMinorPatientIsNull()
    {
        // Arrange
        var args = CreateArgs();
        var context = CreateValidContext();

        // Act
        var act = () => MinorFamilyLinkService.Link(null!, args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Link_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange
        var minorPatient = CreateMinorPatient();
        var context = CreateValidContext();

        // Act
        var act = () => MinorFamilyLinkService.Link(minorPatient, null!, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Link_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var minorPatient = CreateMinorPatient();
        var args = CreateArgs();

        // Arrange & Act
        var act = () => MinorFamilyLinkService.Link(minorPatient, args, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Link_ShouldThrowArgumentNullException_WhenGuardianSelfMembershipIsNull()
    {
        // Arrange
        var minorPatient = CreateMinorPatient();
        var args = CreateArgs();

        var context = CreateValidContext() with { GuardianSelfMembership = null! };

        // Act
        var act = () => MinorFamilyLinkService.Link(minorPatient, args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Link_ShouldThrowDomainValidationException_WhenPatientAlreadyHasActiveMembershipWithGuardian()
    {
        // Arrange
        var minorPatient = CreateMinorPatient();
        var args = CreateArgs();

        var context = CreateValidContext() with { HasActiveMembershipWithGuardian = true };

        // Act
        var act = () => MinorFamilyLinkService.Link(minorPatient, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.PatientAlreadyHasActiveMembership);
    }

    [Fact]
    public void Link_ShouldThrowDomainValidationException_WhenMinorIsExactlyAdultAge()
    {
        // Arrange
        var minorPatient = CreateAdultPatient();
        var args = CreateArgs();
        var context = CreateValidContext();

        // Act
        var act = () => MinorFamilyLinkService.Link(minorPatient, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.PatientRegistration.MinorRequired);
    }

    [Fact]
    public void Link_ShouldThrowDomainValidationException_WhenGuardianSelfMembershipIsNotActive()
    {
        // Arrange
        var minorPatient = CreateMinorPatient();
        var args = CreateArgs();
        var context = CreateValidContext();

        var revokedMembershipReferenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        var revokedMembership = FamilyMembership.CreateAdultFamilyMember(
            context.GuardianPatient.Id,
            Guid.CreateVersion7(),
            PatientRelationship.Other,
            FamilyMembershipAccessLevel.Full,
            DomainRules.AdultAge,
            revokedMembershipReferenceTime
        );

        revokedMembership.Revoke(
            patientHasOwnSelfMembership: true,
            hasUpcomingAppointmentRequiringGuardianForMinor: false,
            revokedMembershipReferenceTime.AddDays(1)
        );

        var invalidContext = context with { GuardianSelfMembership = revokedMembership };

        // Act
        var act = () => MinorFamilyLinkService.Link(minorPatient, args, invalidContext);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.PrimaryPatientRequired);
    }

    [Fact]
    public void Link_ShouldThrowDomainValidationException_WhenGuardianIsUnderAdultAge()
    {
        // Arrange
        var minorPatient = CreateMinorPatient();
        var args = CreateArgs();
        var context = CreateValidContext() with { GuardianPatient = CreateMinorPatient() };

        // Act
        var act = () => MinorFamilyLinkService.Link(minorPatient, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.OwnerMustBeAdult);
    }

    [Fact]
    public void Link_ShouldThrowDomainValidationException_WhenActiveFamilyMemberCountReachesMaximum()
    {
        // Arrange
        var minorPatient = CreateMinorPatient();
        var args = CreateArgs();
        var context = CreateValidContext() with
        {
            ActiveFamilyMemberCount = MinorFamilyLinkService.MaxActiveFamilyMembers,
        };

        // Act
        var act = () => MinorFamilyLinkService.Link(minorPatient, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.MaxActiveFamilyMembersExceeded);
    }

    [Fact]
    public void Link_ShouldCreateFullAccessMembershipWithDomainEvent_WhenValidParameters()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var minorPatient = CreateMinorPatient();
        var guardianPatient = CreateAdultPatient();

        var guardianUserId = Guid.CreateVersion7();

        var args = new MinorFamilyLinkArgs
        {
            GuardianUserId = guardianUserId,
            Relationship = PatientRelationship.Parent,
            LegalAuthority = LegalAuthorityType.Parent,
        };
        var context = new MinorFamilyLinkContext
        {
            GuardianPatient = guardianPatient,
            GuardianSelfMembership = FamilyMembership.CreateSelf(
                guardianPatient.Id,
                guardianUserId,
                referenceTime
            ),
            ActiveFamilyMemberCount = 2,
            HasActiveMembershipWithGuardian = false,
            ReferenceTime = referenceTime,
        };

        // Act
        var membership = MinorFamilyLinkService.Link(minorPatient, args, context);

        // Assert
        membership.PatientId.Should().Be(minorPatient.Id);
        membership.UserId.Should().Be(guardianUserId);
        membership.Role.Should().Be(PatientRelationship.Parent);
        membership.LegalAuthority.Should().Be(LegalAuthorityType.Parent);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.StartedAt.Should().Be(referenceTime);
        membership.EndedAt.Should().BeNull();

        var domainEvent = membership
            .DomainEvents.OfType<MinorLinkedToGuardianEvent>()
            .Should()
            .ContainSingle()
            .Subject;

        domainEvent.MinorPatientId.Should().Be(minorPatient.Id);
        domainEvent.GuardianUserId.Should().Be(guardianUserId);
        domainEvent.Relationship.Should().Be(PatientRelationship.Parent);
        domainEvent.LinkedAt.Should().Be(referenceTime);
    }

    [Fact]
    public void Link_ShouldCreateMembership_WhenGuardianIsExactlyAdultAge()
    {
        // Arrange
        var minorPatient = CreateMinorPatient();
        var guardianPatient = CreateAdultPatient();

        var args = CreateArgs();
        var context = CreateValidContext() with { GuardianPatient = guardianPatient };

        // Act
        var membership = MinorFamilyLinkService.Link(minorPatient, args, context);

        // Assert
        membership.Should().NotBeNull();
        membership.DomainEvents.OfType<MinorLinkedToGuardianEvent>().Should().ContainSingle();
    }

    private Patient CreateMinorPatient() =>
        Patient.CreateProfile(
            PersonName.Create("Apple", null, "Bloom"),
            DateOnly
                .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
                .AddYears(-DomainRules.AdultAge + 1),
            BiologicalSex.Female,
            Address.Create(
                "Sweet Apple Acres",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private Patient CreateAdultPatient() =>
        Patient.CreateProfile(
            PersonName.Create("Applejack", null, "test"),
            DateOnly
                .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
                .AddYears(-DomainRules.AdultAge),
            BiologicalSex.Female,
            Address.Create(
                "Sweet Apple Acres",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private static MinorFamilyLinkArgs CreateArgs() =>
        new()
        {
            GuardianUserId = Guid.CreateVersion7(),
            Relationship = PatientRelationship.Parent,
            LegalAuthority = LegalAuthorityType.Parent,
        };

    private MinorFamilyLinkContext CreateValidContext()
    {
        var guardianPatient = CreateAdultPatient();
        return new()
        {
            GuardianPatient = guardianPatient,
            GuardianSelfMembership = FamilyMembership.CreateSelf(
                guardianPatient.Id,
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            ActiveFamilyMemberCount = 2,
            HasActiveMembershipWithGuardian = false,
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };
    }
}

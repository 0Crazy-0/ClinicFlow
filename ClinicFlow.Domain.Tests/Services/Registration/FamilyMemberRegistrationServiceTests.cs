using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Registration;

public class FamilyMemberRegistrationServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Register_ShouldCreatePatientAndFamilyMembership_WhenNoExistingPatientOrMembership()
    {
        // Arrange
        var ownerUserId = Guid.CreateVersion7();
        var args = CreateArgs(ownerUserId, PatientRelationship.Child);

        // Act
        var (patient, membership) = FamilyMemberRegistrationService.Register(args);

        // Assert
        patient.FullName.Should().Be(args.FullName);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Child);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(args.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldReusePatientAndCreateFamilyMembership_WhenExistingPatientProvidedAndNotDeleted()
    {
        // Arrange
        var existingPatient = Patient.CreateProfile(
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var ownerUserId = Guid.CreateVersion7();
        var args = CreateArgs(ownerUserId, PatientRelationship.Spouse) with
        {
            ExistingPatient = existingPatient,
        };

        // Act
        var (patient, membership) = FamilyMemberRegistrationService.Register(args);

        // Assert
        patient.Should().BeEquivalentTo(existingPatient);

        membership.PatientId.Should().Be(existingPatient.Id);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Spouse);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(args.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldSucceed_WhenOwnerAgeInYearsIsExactly18()
    {
        // Arrange
        var ownerUserId = Guid.CreateVersion7();
        var args = CreateArgs(ownerUserId, PatientRelationship.Child) with { OwnerAgeInYears = 18 };

        // Act
        var (patient, membership) = FamilyMemberRegistrationService.Register(args);

        // Assert
        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Child);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(args.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldThrowOwnerMustBeAdult_WhenOwnerAgeInYearsIsLessThan18()
    {
        // Arrange
        var ownerUserId = Guid.CreateVersion7();
        var args = CreateArgs(ownerUserId, PatientRelationship.Child) with { OwnerAgeInYears = 17 };

        // Act
        var act = () => FamilyMemberRegistrationService.Register(args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.OwnerMustBeAdult);
    }

    [Fact]
    public void Register_ShouldThrowPatientAlreadyHasActiveMembership_WhenHasExistingMembershipWithOwnerIsTrue()
    {
        // Arrange
        var ownerUserId = Guid.CreateVersion7();
        var args = CreateArgs(ownerUserId, PatientRelationship.Child) with
        {
            HasExistingMembershipWithOwner = true,
        };

        // Act
        var act = () => FamilyMemberRegistrationService.Register(args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.PatientAlreadyHasActiveMembership);
    }

    [Fact]
    public void Register_ShouldThrowMaxActiveFamilyMembersExceeded_WhenActiveFamilyMemberCountReachesMax()
    {
        // Arrange
        var ownerUserId = Guid.CreateVersion7();
        var args = CreateArgs(ownerUserId, PatientRelationship.Child) with
        {
            ActiveFamilyMemberCount = FamilyMemberRegistrationService.MaxActiveFamilyMembers,
        };

        // Act
        var act = () => FamilyMemberRegistrationService.Register(args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.MaxActiveFamilyMembersExceeded);
    }

    private FamilyMemberRegistrationArgs CreateArgs(Guid ownerUserId, PatientRelationship role) =>
        new()
        {
            OwnerAgeInYears = 25,
            OwnerUserId = ownerUserId,
            Role = role,
            FullName = PersonName.Create("Test Patient"),
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };
}

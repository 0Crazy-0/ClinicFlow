using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts;
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
        var args = CreateValidArgs(PatientRelationship.Child);
        var context = CreateValidContext(ownerUserId);

        // Act
        var (patient, membership) = FamilyMemberRegistrationService.Register(args, context);

        // Assert
        patient.FullName.Should().Be(args.FullName);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Child);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.StartedAt.Should().Be(context.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldReusePatientAndCreateFamilyMembership_WhenExistingPatientProvidedAndNotDeleted()
    {
        // Arrange
        var existingPatient = Patient.CreateProfile(
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var ownerUserId = Guid.CreateVersion7();
        var args = CreateValidArgs(PatientRelationship.Spouse);
        var context = CreateValidContext(ownerUserId) with { ExistingPatient = existingPatient };

        // Act
        var (patient, membership) = FamilyMemberRegistrationService.Register(args, context);

        // Assert
        patient.Should().BeEquivalentTo(existingPatient);

        membership.PatientId.Should().Be(existingPatient.Id);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Spouse);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.StartedAt.Should().Be(context.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldSucceed_WhenOwnerAgeInYearsIsExactly18()
    {
        // Arrange
        var ownerUserId = Guid.CreateVersion7();
        var args = CreateValidArgs(PatientRelationship.Child);
        var context = CreateValidContext(ownerUserId) with { OwnerAgeInYears = 18 };

        // Act
        var (patient, membership) = FamilyMemberRegistrationService.Register(args, context);

        // Assert
        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Child);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.StartedAt.Should().Be(context.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            FamilyMemberRegistrationService.Register(
                null!,
                CreateValidContext(Guid.CreateVersion7())
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenFullNameIsNull()
    {
        // Arrange
        var args = CreateValidArgs(PatientRelationship.Child) with
        {
            FullName = null!,
        };

        // Act
        var act = () =>
            FamilyMemberRegistrationService.Register(
                args,
                CreateValidContext(Guid.CreateVersion7())
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var args = CreateValidArgs(PatientRelationship.Child);

        // Act
        var act = () => FamilyMemberRegistrationService.Register(args, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowOwnerMustBeAdult_WhenOwnerAgeInYearsIsLessThan18()
    {
        // Arrange
        var ownerUserId = Guid.CreateVersion7();
        var args = CreateValidArgs(PatientRelationship.Child);
        var context = CreateValidContext(ownerUserId) with { OwnerAgeInYears = 17 };

        // Act
        var act = () => FamilyMemberRegistrationService.Register(args, context);

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
        var args = CreateValidArgs(PatientRelationship.Child);
        var context = CreateValidContext(ownerUserId) with
        {
            HasExistingMembershipWithOwner = true,
        };

        // Act
        var act = () => FamilyMemberRegistrationService.Register(args, context);

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
        var args = CreateValidArgs(PatientRelationship.Child);
        var context = CreateValidContext(ownerUserId) with
        {
            ActiveFamilyMemberCount = FamilyMemberRegistrationService.MaxActiveFamilyMembers,
        };

        // Act
        var act = () => FamilyMemberRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.MaxActiveFamilyMembersExceeded);
    }

    [Fact]
    public void Register_ShouldThrowDataMismatch_WhenExistingPatientSexDiffers()
    {
        // Arrange
        var existingPatient = Patient.CreateProfile(
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var ownerUserId = Guid.CreateVersion7();
        var args = CreateValidArgs(PatientRelationship.Child) with
        {
            SexAtBirth = BiologicalSex.Female,
        };
        var context = CreateValidContext(ownerUserId) with { ExistingPatient = existingPatient };

        // Act
        var act = () => FamilyMemberRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.DataMismatch);
    }

    private FamilyMemberRegistrationArgs CreateValidArgs(PatientRelationship role) =>
        new()
        {
            Role = role,
            AccessLevel = FamilyMembershipAccessLevel.Full,
            FullName = PersonName.Create("Test Patient"),
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            SexAtBirth = BiologicalSex.Male,
        };

    private FamilyMemberRegistrationContext CreateValidContext(Guid ownerUserId) =>
        new()
        {
            OwnerAgeInYears = 25,
            OwnerUserId = ownerUserId,
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };
}

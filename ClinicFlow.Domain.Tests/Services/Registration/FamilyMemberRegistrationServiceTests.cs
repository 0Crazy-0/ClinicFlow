using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Patients;
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
    public void Register_ShouldCreateFamilyMember_WhenNoExistingProfile()
    {
        // Arrange
        var args = CreateArgs(PatientRelationship.Sibling, Guid.CreateVersion7());

        // Act
        var result = FamilyMemberRegistrationService.Register(null, 0, args);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(args.UserId);
        result.FullName.Should().Be(args.FullName);
        result.DateOfBirth.Should().Be(args.DateOfBirth);
        result.RelationshipToUser.Should().Be(PatientRelationship.Sibling);
    }

    [Fact]
    public void Register_ShouldThrowFamilyMemberLimitExceeded_WhenActiveFamilyMemberCountReachesMax()
    {
        // Arrange
        var args = CreateArgs(PatientRelationship.Sibling, Guid.CreateVersion7());

        // Act
        var act = () =>
            FamilyMemberRegistrationService.Register(
                null,
                FamilyMemberRegistrationService.MaxActiveFamilyMembers,
                args
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.FamilyMemberLimitExceeded);
    }

    [Fact]
    public void Register_ShouldThrowAlreadyExists_WhenActiveProfileExists()
    {
        // Arrange
        var existingProfile = Patient.CreateFamilyMember(
            Guid.CreateVersion7(),
            PersonName.Create("Test Patient"),
            PatientRelationship.Sibling,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = CreateArgs(PatientRelationship.Sibling, existingProfile.UserId);

        // Act
        var act = () => FamilyMemberRegistrationService.Register(existingProfile, 0, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.ActiveProfileAlreadyExists);
    }

    [Fact]
    public void Register_ShouldThrowUserIdMismatch_WhenUserIdsDoNotMatch()
    {
        // Arrange
        var existingProfile = CreateDeletedPatient();
        var args = new FamilyMemberRegistrationArgs
        {
            UserId = Guid.CreateVersion7(),
            FullName = PersonName.Create("Test Patient"),
            Relationship = PatientRelationship.Sibling,
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => FamilyMemberRegistrationService.Register(existingProfile, 0, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.UserIdMismatch);
    }

    [Fact]
    public void Register_ShouldReactivateProfile_WhenDeletedProfileExists()
    {
        // Arrange
        var deletedProfile = CreateDeletedPatient();
        var args = CreateArgs(PatientRelationship.Other, deletedProfile.UserId);

        // Act
        var result = FamilyMemberRegistrationService.Register(deletedProfile, 0, args);

        // Assert
        result.Should().BeSameAs(deletedProfile);
        result.IsDeleted.Should().BeFalse();
        result.RelationshipToUser.Should().Be(PatientRelationship.Other);
    }

    [Fact]
    public void Register_ShouldEmitReactivatedEvent_WhenDeletedProfileExists()
    {
        // Arrange
        var deletedProfile = CreateDeletedPatient();
        var args = CreateArgs(PatientRelationship.Other, deletedProfile.UserId);

        // Act
        FamilyMemberRegistrationService.Register(deletedProfile, 0, args);

        // Assert
        deletedProfile.DomainEvents.OfType<PatientReactivatedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Register_ShouldThrowFamilyMemberLimitExceeded_WhenReactivatingDeletedProfileAtLimit()
    {
        // Arrange
        var deletedProfile = CreateDeletedPatient();
        var args = CreateArgs(PatientRelationship.Other, deletedProfile.UserId);

        // Act
        var act = () =>
            FamilyMemberRegistrationService.Register(
                deletedProfile,
                FamilyMemberRegistrationService.MaxActiveFamilyMembers,
                args
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.FamilyMemberLimitExceeded);
    }

    private FamilyMemberRegistrationArgs CreateArgs(
        PatientRelationship relationship,
        Guid userId
    ) =>
        new()
        {
            UserId = userId,
            FullName = PersonName.Create("Test Patient"),
            Relationship = relationship,
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };

    private Patient CreateDeletedPatient()
    {
        var userId = Guid.CreateVersion7();
        var patient = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Test Patient"),
            PatientRelationship.Sibling,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.RemoveFamilyMember(userId);

        return patient;
    }
}

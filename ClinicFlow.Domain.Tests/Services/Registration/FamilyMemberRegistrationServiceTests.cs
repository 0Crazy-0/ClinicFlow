using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Registration;

public class FamilyMemberRegistrationServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Register_ShouldCreateFamilyMember_WhenNoExistingProfile()
    {
        // Arrange
        var args = CreateArgs(PatientRelationship.Sibling, Guid.NewGuid());

        // Act
        var result = FamilyMemberRegistrationService.Register(null, args);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(args.UserId);
        result.FullName.Should().Be(args.FullName);
        result.DateOfBirth.Should().Be(args.DateOfBirth);
        result.RelationshipToUser.Should().Be(PatientRelationship.Sibling);
    }

    [Fact]
    public void Register_ShouldThrowAlreadyExists_WhenActiveProfileExists()
    {
        // Arrange
        var existingProfile = CreateActivePatient(PatientRelationship.Sibling, Guid.NewGuid());
        var args = CreateArgs(PatientRelationship.Sibling, existingProfile.UserId);

        // Act
        var act = () => FamilyMemberRegistrationService.Register(existingProfile, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.ActiveProfileAlreadyExists);
    }

    [Fact]
    public void Register_ShouldThrowUserIdMismatch_WhenUserIdsDoNotMatch()
    {
        // Arrange
        var existingProfile = CreateDeletedPatient(PatientRelationship.Sibling, Guid.NewGuid());
        var args = new FamilyMemberRegistrationArgs
        {
            UserId = Guid.NewGuid(),
            FullName = PersonName.Create("Test Patient"),
            Relationship = PatientRelationship.Sibling,
            DateOfBirth = _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => FamilyMemberRegistrationService.Register(existingProfile, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.UserIdMismatch);
    }

    [Fact]
    public void Register_ShouldReactivateProfile_WhenDeletedProfileExists()
    {
        // Arrange
        var deletedProfile = CreateDeletedPatient(PatientRelationship.Sibling, Guid.NewGuid());
        var args = CreateArgs(PatientRelationship.Other, deletedProfile.UserId);

        // Act
        var result = FamilyMemberRegistrationService.Register(deletedProfile, args);

        // Assert
        result.Should().BeSameAs(deletedProfile);
        result.IsDeleted.Should().BeFalse();
        result.RelationshipToUser.Should().Be(PatientRelationship.Other);
    }

    [Fact]
    public void Register_ShouldEmitReactivatedEvent_WhenDeletedProfileExists()
    {
        // Arrange
        var deletedProfile = CreateDeletedPatient(PatientRelationship.Sibling, Guid.NewGuid());
        var args = CreateArgs(PatientRelationship.Other, deletedProfile.UserId);

        // Act
        FamilyMemberRegistrationService.Register(deletedProfile, args);

        // Assert
        deletedProfile.DomainEvents.OfType<PatientReactivatedEvent>().Should().ContainSingle();
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
            DateOfBirth = _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };

    private Patient CreateActivePatient(PatientRelationship relationship, Guid userId) =>
        Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Test Patient"),
            relationship,
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private Patient CreateDeletedPatient(PatientRelationship relationship, Guid userId)
    {
        var patient = CreateActivePatient(relationship, userId);
        patient.MarkAsDeleted();
        return patient;
    }
}

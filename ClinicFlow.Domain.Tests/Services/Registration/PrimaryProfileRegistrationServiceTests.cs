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

public class PrimaryProfileRegistrationServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Register_ShouldCreateSelfProfile_WhenNoExistingProfile()
    {
        // Arrange
        var args = CreateArgs(Guid.CreateVersion7());

        // Act
        var result = PrimaryProfileRegistrationService.Register(null, args);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(args.UserId);
        result.FullName.Should().Be(args.FullName);
        result.DateOfBirth.Should().Be(args.DateOfBirth);
        result.RelationshipToUser.Should().Be(PatientRelationship.Self);
    }

    [Fact]
    public void Register_ShouldThrowAlreadyExists_WhenActiveProfileExists()
    {
        // Arrange
        var existingProfile = Patient.CreateSelf(
            Guid.CreateVersion7(),
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var args = CreateArgs(existingProfile.UserId);

        // Act
        var act = () => PrimaryProfileRegistrationService.Register(existingProfile, args);

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
        var args = new PrimaryProfileRegistrationArgs
        {
            UserId = Guid.CreateVersion7(),
            FullName = PersonName.Create("Test Patient"),
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => PrimaryProfileRegistrationService.Register(existingProfile, args);

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
        var args = CreateArgs(deletedProfile.UserId);

        // Act
        var result = PrimaryProfileRegistrationService.Register(deletedProfile, args);

        // Assert
        result.Should().BeSameAs(deletedProfile);
        result.IsDeleted.Should().BeFalse();
        result.RelationshipToUser.Should().Be(PatientRelationship.Self);
    }

    [Fact]
    public void Register_ShouldEmitReactivatedEvent_WhenDeletedProfileExists()
    {
        // Arrange
        var deletedProfile = CreateDeletedPatient();
        var args = CreateArgs(deletedProfile.UserId);

        // Act
        PrimaryProfileRegistrationService.Register(deletedProfile, args);

        // Assert
        deletedProfile.DomainEvents.OfType<PatientReactivatedEvent>().Should().ContainSingle();
    }

    private PrimaryProfileRegistrationArgs CreateArgs(Guid userId) =>
        new()
        {
            UserId = userId,
            FullName = PersonName.Create("Test Patient"),
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };

    private Patient CreateDeletedPatient()
    {
        var patient = Patient.CreateSelf(
            Guid.CreateVersion7(),
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.CloseAccount(false);
        return patient;
    }
}

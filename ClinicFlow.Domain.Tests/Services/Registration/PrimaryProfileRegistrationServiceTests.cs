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

public class PrimaryProfileRegistrationServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Register_ShouldCreatePatientAndSelfMembership_WhenNoExistingPatientOrMembership()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var args = CreateArgs(userId);

        // Act
        var (patient, membership) = PrimaryProfileRegistrationService.Register(args);

        // Assert
        patient.FullName.Should().Be(args.FullName);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(userId);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(args.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldReusePatientAndCreateSelfMembership_WhenExistingPatientProvidedAndNotDeleted()
    {
        // Arrange
        var existingPatient = Patient.CreateProfile(
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var userId = Guid.CreateVersion7();
        var args = CreateArgs(userId) with { ExistingPatient = existingPatient };

        // Act
        var (patient, membership) = PrimaryProfileRegistrationService.Register(args);

        // Assert
        patient.Should().BeEquivalentTo(existingPatient);

        membership.PatientId.Should().Be(existingPatient.Id);
        membership.UserId.Should().Be(userId);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(args.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldThrowProfileRequiresAdministrativeClaim_WhenExistingPatientIsDeleted()
    {
        // Arrange
        var deletedPatient = Patient.CreateProfile(
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        deletedPatient.CloseAccount();

        var args = CreateArgs(Guid.CreateVersion7()) with { ExistingPatient = deletedPatient };

        // Act
        var act = () => PrimaryProfileRegistrationService.Register(args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.ProfileRequiresAdministrativeClaim);
    }

    [Fact]
    public void Register_ShouldThrowPatientAlreadyHasActiveMembership_WhenHasExistingSelfMembershipIsTrue()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var args = CreateArgs(userId) with { HasExistingSelfMembership = true };

        // Act
        var act = () => PrimaryProfileRegistrationService.Register(args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.PatientAlreadyHasActiveMembership);
    }

    private PrimaryProfileRegistrationArgs CreateArgs(Guid userId) =>
        new()
        {
            UserId = userId,
            FullName = PersonName.Create("Test Patient"),
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };
}

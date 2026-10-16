using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;
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
        var args = CreateValidArgs();
        var context = CreateValidContext(userId);

        // Act
        var (patient, membership) = PrimaryProfileRegistrationService.Register(args, context);

        // Assert
        patient.FullName.Should().Be(args.FullName);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);
        patient.SexAtBirth.Should().Be(args.SexAtBirth);

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(userId);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(context.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldReusePatientAndCreateSelfMembership_WhenExistingPatientProvidedAndNotDeleted()
    {
        // Arrange
        var existingPatient = Patient.CreateProfile(
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var userId = Guid.CreateVersion7();
        var args = CreateValidArgs();
        var context = CreateValidContext(userId) with { ExistingPatient = existingPatient };

        // Act
        var (patient, membership) = PrimaryProfileRegistrationService.Register(args, context);

        // Assert
        patient.Should().BeEquivalentTo(existingPatient);

        membership.PatientId.Should().Be(existingPatient.Id);
        membership.UserId.Should().Be(userId);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(context.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldSucceed_WhenPatientAgeIsExactly13()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var args = CreateValidArgs() with
        {
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-13)),
        };
        var context = CreateValidContext(userId);

        // Act
        var (patient, membership) = PrimaryProfileRegistrationService.Register(args, context);

        // Assert
        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(userId);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(context.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldSucceed_WhenPatientAgeIsExactly12()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var args = CreateValidArgs() with
        {
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-12)),
        };
        var context = CreateValidContext(userId);

        // Act
        var (patient, membership) = PrimaryProfileRegistrationService.Register(args, context);

        // Assert
        patient.GetAge(DateOnly.FromDateTime(context.ReferenceTime)).Should().Be(12);
        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(userId);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(context.ReferenceTime);
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            PrimaryProfileRegistrationService.Register(
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
        var args = CreateValidArgs() with
        {
            FullName = null!,
        };

        // Act
        var act = () =>
            PrimaryProfileRegistrationService.Register(
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
        var args = CreateValidArgs();

        // Act
        var act = () => PrimaryProfileRegistrationService.Register(args, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowPatientAlreadyHasActiveMembership_WhenHasExistingSelfMembershipIsTrue()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var args = CreateValidArgs();
        var context = CreateValidContext(userId) with { HasExistingSelfMembership = true };

        // Act
        var act = () => PrimaryProfileRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.PatientAlreadyHasActiveMembership);
    }

    [Fact]
    public void Register_ShouldThrowPatientTooYoungForSelfMembership_WhenNewPatientAgeIsLessThan12()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var args = CreateValidArgs() with
        {
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-11)),
        };
        var context = CreateValidContext(userId);

        // Act
        var act = () => PrimaryProfileRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.PatientTooYoungForSelfMembership);
    }

    [Fact]
    public void Register_ShouldThrowPatientTooYoungForSelfMembership_WhenExistingPatientAgeIsLessThan12()
    {
        // Arrange
        var existingPatient = Patient.CreateProfile(
            PersonName.Create("Test Patient"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-11)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var userId = Guid.CreateVersion7();
        var args = CreateValidArgs();
        var context = CreateValidContext(userId) with { ExistingPatient = existingPatient };

        // Act
        var act = () => PrimaryProfileRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.PatientTooYoungForSelfMembership);
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

        var userId = Guid.CreateVersion7();
        var args = CreateValidArgs() with { SexAtBirth = BiologicalSex.Female };
        var context = CreateValidContext(userId) with { ExistingPatient = existingPatient };

        // Act
        var act = () => PrimaryProfileRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.DataMismatch);
    }

    private PrimaryProfileRegistrationArgs CreateValidArgs() =>
        new()
        {
            FullName = PersonName.Create("Test Patient"),
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            SexAtBirth = BiologicalSex.Male,
        };

    private PrimaryProfileRegistrationContext CreateValidContext(Guid userId) =>
        new() { UserId = userId, ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime };
}

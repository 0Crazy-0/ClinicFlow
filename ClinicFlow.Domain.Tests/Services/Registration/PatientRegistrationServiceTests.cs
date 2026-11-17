using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Patients;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Registration;

public class PatientRegistrationServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Register_ShouldCreatePatientUserAndSelfMembership_WhenValidArgs()
    {
        // Arrange
        var args = CreateValidArgs();
        var context = CreateValidContext();

        // Act
        var (patient, returnedUser, membership) = PatientRegistrationService.Register(
            args,
            context
        );

        // Assert
        patient.FullName.FirstName.Should().Be(args.FirstName);
        patient.FullName.FirstSurname.Should().Be(args.FirstSurname);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);
        patient.SexAtBirth.Should().Be(args.SexAtBirth);
        patient.HomeAddress.Should().BeEquivalentTo(args.HomeAddress);
        patient.DuplicateOverrideReason.Should().BeNull();

        returnedUser.Should().BeEquivalentTo(context.PendingUser);
        returnedUser.PasswordHash.Should().BeNull();
        returnedUser.ActivatedAt.Should().BeNull();

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(context.PendingUser.Id);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.LegalAuthority.Should().Be(LegalAuthorityType.None);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);

        var domainEvent = patient
            .DomainEvents.OfType<PatientRegisteredByReceptionEvent>()
            .Should()
            .ContainSingle()
            .Subject;

        domainEvent.PatientId.Should().Be(patient.Id);
        domainEvent.ReceptionistUserId.Should().Be(context.ReceptionistUserId);
        domainEvent.RegisteredAt.Should().Be(_fakeTime.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Register_ShouldRegisterPatient_WhenAgeIsExactly12()
    {
        // Arrange
        var patientDob = DateOnly
            .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            .AddYears(-PatientRegistrationService.MinimumSelfMembershipAge);

        var args = CreateValidArgs() with { DateOfBirth = patientDob };
        var context = CreateValidContext();

        // Act
        var (patient, returnedUser, membership) = PatientRegistrationService.Register(
            args,
            context
        );

        // Assert
        patient.FullName.FirstName.Should().Be(args.FirstName);
        patient.FullName.FirstSurname.Should().Be(args.FirstSurname);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);
        patient.SexAtBirth.Should().Be(args.SexAtBirth);
        patient.HomeAddress.Should().BeEquivalentTo(args.HomeAddress);
        patient.DuplicateOverrideReason.Should().BeNull();

        returnedUser.Should().BeEquivalentTo(context.PendingUser);
        returnedUser.PasswordHash.Should().BeNull();
        returnedUser.ActivatedAt.Should().BeNull();

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(context.PendingUser.Id);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.LegalAuthority.Should().Be(LegalAuthorityType.None);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);

        var domainEvent = patient
            .DomainEvents.OfType<PatientRegisteredByReceptionEvent>()
            .Should()
            .ContainSingle()
            .Subject;

        domainEvent.PatientId.Should().Be(patient.Id);
        domainEvent.ReceptionistUserId.Should().Be(context.ReceptionistUserId);
        domainEvent.RegisteredAt.Should().Be(_fakeTime.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Register_ShouldRegisterPatient_WhenAgeIsExactly18()
    {
        // Arrange
        var patientDob = DateOnly
            .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            .AddYears(-DomainRules.AdultAge);

        var args = CreateValidArgs() with { DateOfBirth = patientDob };
        var context = CreateValidContext();

        // Act
        var (patient, returnedUser, membership) = PatientRegistrationService.Register(
            args,
            context
        );

        // Assert
        patient.FullName.FirstName.Should().Be(args.FirstName);
        patient.FullName.FirstSurname.Should().Be(args.FirstSurname);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);
        patient.SexAtBirth.Should().Be(args.SexAtBirth);
        patient.HomeAddress.Should().BeEquivalentTo(args.HomeAddress);
        patient.DuplicateOverrideReason.Should().BeNull();

        returnedUser.Should().BeEquivalentTo(context.PendingUser);
        returnedUser.PasswordHash.Should().BeNull();
        returnedUser.ActivatedAt.Should().BeNull();

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(context.PendingUser.Id);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.LegalAuthority.Should().Be(LegalAuthorityType.None);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);

        var domainEvent = patient
            .DomainEvents.OfType<PatientRegisteredByReceptionEvent>()
            .Should()
            .ContainSingle()
            .Subject;

        domainEvent.PatientId.Should().Be(patient.Id);
        domainEvent.ReceptionistUserId.Should().Be(context.ReceptionistUserId);
        domainEvent.RegisteredAt.Should().Be(_fakeTime.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Register_ShouldThrowDomainValidationException_WhenPatientIsUnder12()
    {
        // Arrange
        var patientDob = DateOnly
            .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            .AddYears(-PatientRegistrationService.MinimumSelfMembershipAge + 1);

        var args = CreateValidArgs() with { DateOfBirth = patientDob };
        var context = CreateValidContext();

        // Act
        var act = () => PatientRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.PatientTooYoungForSelfMembership);
    }

    [Fact]
    public void Register_ShouldPersistOverrideReason_WhenHighScoreWithOverrideReason()
    {
        // Arrange
        var args = CreateValidArgs() with { DuplicateOverrideReason = "A crazy reason" };
        var duplicateCandidates = CreateMatchingCandidate();

        var context = CreateValidContext() with { DuplicateCandidates = [duplicateCandidates] };

        // Act
        var (patient, returnedUser, membership) = PatientRegistrationService.Register(
            args,
            context
        );

        // Assert
        patient.FullName.FirstName.Should().Be(args.FirstName);
        patient.FullName.FirstSurname.Should().Be(args.FirstSurname);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);
        patient.SexAtBirth.Should().Be(args.SexAtBirth);
        patient.HomeAddress.Should().BeEquivalentTo(args.HomeAddress);
        patient.DuplicateOverrideReason.Should().Be(args.DuplicateOverrideReason);

        returnedUser.Should().BeEquivalentTo(context.PendingUser);
        returnedUser.PasswordHash.Should().BeNull();
        returnedUser.ActivatedAt.Should().BeNull();

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(context.PendingUser.Id);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.LegalAuthority.Should().Be(LegalAuthorityType.None);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);

        var domainEvent = patient
            .DomainEvents.OfType<PatientRegisteredByReceptionEvent>()
            .Should()
            .ContainSingle()
            .Subject;

        domainEvent.PatientId.Should().Be(patient.Id);
        domainEvent.ReceptionistUserId.Should().Be(context.ReceptionistUserId);
        domainEvent.RegisteredAt.Should().Be(_fakeTime.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange
        var context = CreateValidContext();

        // Act
        var act = () => PatientRegistrationService.Register(null!, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenFirstNameIsNull()
    {
        // Arrange
        var args = CreateValidArgs() with { FirstName = null! };
        var context = CreateValidContext();

        // Act
        var act = () => PatientRegistrationService.Register(args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenFirstSurnameIsNull()
    {
        // Arrange
        var args = CreateValidArgs() with { FirstSurname = null! };
        var context = CreateValidContext();

        // Act
        var act = () => PatientRegistrationService.Register(args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenHomeAddressIsNull_EvenForMinor()
    {
        // Arrange
        var args = CreateValidArgs() with
        {
            HomeAddress = null!,
            DateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime).AddYears(-10),
        };
        var context = CreateValidContext();

        // Act
        var act = () => PatientRegistrationService.Register(args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("args.HomeAddress");
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenEmailIsNull()
    {
        // Arrange
        var args = CreateValidArgs() with { Email = null! };
        var context = CreateValidContext();

        // Act
        var act = () => PatientRegistrationService.Register(args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenPhoneIsNull()
    {
        // Arrange
        var args = CreateValidArgs() with { Phone = null! };
        var context = CreateValidContext();

        // Act
        var act = () => PatientRegistrationService.Register(args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var args = CreateValidArgs();

        // Act
        var act = () => PatientRegistrationService.Register(args, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenDuplicateCandidatesIsNull()
    {
        // Arrange
        var args = CreateValidArgs();
        var context = CreateValidContext() with { DuplicateCandidates = null! };

        // Act
        var act = () => PatientRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("context.DuplicateCandidates");
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenPendingUserIsNull()
    {
        // Arrange
        var args = CreateValidArgs();
        var context = CreateValidContext() with { PendingUser = null! };

        // Act
        var act = () => PatientRegistrationService.Register(args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowDomainValidationException_WhenReceptionistUserIdIsEmpty()
    {
        // Arrange
        var args = CreateValidArgs();
        var context = CreateValidContext() with { ReceptionistUserId = Guid.Empty };

        // Act
        var act = () => PatientRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Register_ShouldThrowDomainValidationException_WhenHighScoreWithoutOverrideReason()
    {
        // Arrange
        var args = CreateValidArgs();
        var duplicateCandidates = CreateMatchingCandidate();

        var context = CreateValidContext() with { DuplicateCandidates = [duplicateCandidates] };

        // Act
        var act = () => PatientRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.PatientRegistration.DuplicateSuspected);
    }

    private PatientRegistrationArgs CreateValidArgs() =>
        new()
        {
            FirstName = "Applejack",
            MiddleName = null,
            FirstSurname = "test",
            DateOfBirth = DateOnly
                .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
                .AddYears(-DomainRules.AdultAge + 1),
            SexAtBirth = BiologicalSex.Female,
            HomeAddress = Address.Create(
                "Sweet Apple Acres",
                AddressState.Texas,
                "75001",
                Country.UnitedStates
            ),
            Email = EmailAddress.Create("AppleFamily@clinic.com"),
            Phone = PhoneNumber.Create("555-1234"),
        };

    private PatientRegistrationContext CreateValidContext() =>
        new()
        {
            DuplicateCandidates = [],
            PendingUser = User.CreatePending(
                EmailAddress.Create("remy@clinic.com"),
                PhoneNumber.Create("555-1234")
            ),
            ReceptionistUserId = Guid.CreateVersion7(),
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };

    private PatientDuplicateCandidate CreateMatchingCandidate() =>
        new()
        {
            PatientId = Guid.CreateVersion7(),
            FullName = PersonName.Create("Applejack", null, "test"),
            DateOfBirth = DateOnly
                .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
                .AddYears(-DomainRules.AdultAge + 1),
            SexAtBirth = BiologicalSex.Female,
            HomeAddress = Address.Create(
                "Sweet Apple Acres",
                AddressState.Texas,
                "75001",
                Country.UnitedStates
            ),
            Email = EmailAddress.Create("AppleFamily@clinic.com"),
            Phone = PhoneNumber.Create("(555) 1234"),
            SsnLast4Matches = false,
        };
}

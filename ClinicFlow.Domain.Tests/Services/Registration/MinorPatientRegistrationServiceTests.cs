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

public class MinorPatientRegistrationServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Register_ShouldCreatePatientAndGuardianMembership_WhenValidMinorArgs()
    {
        // Arrange
        var args = CreateMinorArgs();
        var context = CreateValidContext();

        // Act
        var (patient, membership) = MinorPatientRegistrationService.Register(args, context);

        // Assert
        patient.FullName.FirstName.Should().Be(args.FirstName);
        patient.FullName.FirstSurname.Should().Be(args.FirstSurname);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);
        patient.SexAtBirth.Should().Be(args.SexAtBirth);
        patient.HomeAddress.Should().BeEquivalentTo(args.HomeAddress);
        patient.DuplicateOverrideReason.Should().BeNull();

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(context.GuardianSelfMembership.UserId);
        membership.Role.Should().Be(args.Relationship);
        membership.LegalAuthority.Should().Be(args.LegalAuthority);
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
    public void Register_ShouldRegisterMinor_WhenAgeIsExactly17()
    {
        // Arrange
        var patientAge = DateOnly
            .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            .AddYears(-DomainRules.AdultAge + 1);

        var args = CreateMinorArgs() with { DateOfBirth = patientAge };
        var context = CreateValidContext();

        // Act
        var (patient, membership) = MinorPatientRegistrationService.Register(args, context);

        // Assert
        patient.FullName.FirstName.Should().Be(args.FirstName);
        patient.FullName.FirstSurname.Should().Be(args.FirstSurname);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);
        patient.SexAtBirth.Should().Be(args.SexAtBirth);
        patient.HomeAddress.Should().BeEquivalentTo(args.HomeAddress);
        patient.DuplicateOverrideReason.Should().BeNull();

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(context.GuardianSelfMembership.UserId);
        membership.Role.Should().Be(args.Relationship);
        membership.LegalAuthority.Should().Be(args.LegalAuthority);
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
    public void Register_ShouldPersistOverrideReason_WhenHighScoreWithOverrideReason()
    {
        // Arrange
        var args = CreateMinorArgs() with { DuplicateOverrideReason = "A crazy reason" };

        var duplicateCandidates = CreateMatchingCandidate();

        var context = CreateValidContext() with { DuplicateCandidates = [duplicateCandidates] };

        // Act
        var (patient, membership) = MinorPatientRegistrationService.Register(args, context);

        // Assert
        patient.FullName.FirstName.Should().Be(args.FirstName);
        patient.FullName.FirstSurname.Should().Be(args.FirstSurname);
        patient.DateOfBirth.Should().Be(args.DateOfBirth);
        patient.SexAtBirth.Should().Be(args.SexAtBirth);
        patient.HomeAddress.Should().BeEquivalentTo(args.HomeAddress);
        patient.DuplicateOverrideReason.Should().Be(args.DuplicateOverrideReason);

        membership.PatientId.Should().Be(patient.Id);
        membership.UserId.Should().Be(context.GuardianSelfMembership.UserId);
        membership.Role.Should().Be(args.Relationship);
        membership.LegalAuthority.Should().Be(args.LegalAuthority);
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
        var act = () => MinorPatientRegistrationService.Register(null!, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenFirstNameIsNull()
    {
        // Arrange
        var args = CreateMinorArgs() with { FirstName = null! };
        var context = CreateValidContext();

        // Act
        var act = () => MinorPatientRegistrationService.Register(args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenFirstSurnameIsNull()
    {
        // Arrange
        var args = CreateMinorArgs() with { FirstSurname = null! };
        var context = CreateValidContext();

        // Act
        var act = () => MinorPatientRegistrationService.Register(args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenHomeAddressIsNull_EvenForAdult()
    {
        // Arrange
        var args = CreateMinorArgs() with
        {
            HomeAddress = null!,
            DateOfBirth = DateOnly
                .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
                .AddYears(-DomainRules.AdultAge),
        };
        var context = CreateValidContext();

        // Act
        var act = () => MinorPatientRegistrationService.Register(args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("args.HomeAddress");
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var args = CreateMinorArgs();

        // Act
        var act = () => MinorPatientRegistrationService.Register(args, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenDuplicateCandidatesIsNull()
    {
        // Arrange
        var args = CreateMinorArgs();
        var context = CreateValidContext() with { DuplicateCandidates = null! };

        // Act
        var act = () => MinorPatientRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("context.DuplicateCandidates");
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenGuardianPatientIsNull()
    {
        // Arrange
        var args = CreateMinorArgs();
        var duplicateCandidates = CreateMatchingCandidate();

        var context = CreateValidContext() with
        {
            DuplicateCandidates = [duplicateCandidates],
            GuardianPatient = null!,
        };

        // Act
        var act = () => MinorPatientRegistrationService.Register(args, context);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("context.GuardianPatient");
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenGuardianSelfMembershipIsNull()
    {
        // Arrange
        var args = CreateMinorArgs();
        var duplicateCandidates = CreateMatchingCandidate();

        var context = CreateValidContext() with
        {
            DuplicateCandidates = [duplicateCandidates],
            GuardianSelfMembership = null!,
        };

        // Act
        var act = () => MinorPatientRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("context.GuardianSelfMembership");
    }

    [Fact]
    public void Register_ShouldThrowDomainValidationException_WhenPatientIsExactlyAdultAge()
    {
        // Arrange
        var adultDateOfBirth = DateOnly
            .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
            .AddYears(-DomainRules.AdultAge);

        var args = CreateMinorArgs() with { DateOfBirth = adultDateOfBirth };

        var duplicateCandidates = CreateMatchingCandidate() with { DateOfBirth = adultDateOfBirth };

        var context = CreateValidContext() with { DuplicateCandidates = [duplicateCandidates] };

        // Act
        var act = () => MinorPatientRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.PatientRegistration.MinorRequired);
    }

    [Fact]
    public void Register_ShouldThrowDomainValidationException_WhenHighScoreWithoutOverrideReason()
    {
        // Arrange
        var args = CreateMinorArgs();
        var duplicateCandidates = CreateMatchingCandidate();

        var context = CreateValidContext() with { DuplicateCandidates = [duplicateCandidates] };

        // Act
        var act = () => MinorPatientRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.PatientRegistration.DuplicateSuspected);
    }

    private MinorPatientRegistrationArgs CreateMinorArgs() =>
        new()
        {
            FirstName = "Apple",
            MiddleName = null,
            FirstSurname = "Bloom",
            DateOfBirth = DateOnly
                .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
                .AddYears(-DomainRules.AdultAge + 1),
            SexAtBirth = BiologicalSex.Female,
            HomeAddress = Address.Create(
                "Sweet Apple Acres",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            HasSsnLast4 = false,
            DuplicateOverrideReason = null,
            Relationship = PatientRelationship.Parent,
            LegalAuthority = LegalAuthorityType.Parent,
        };

    private MinorPatientRegistrationContext CreateValidContext()
    {
        var guardianPatient = Patient.CreateProfile(
            PersonName.Create("Applejack", null, "test"),
            DateOnly
                .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
                .AddYears(-DomainRules.AdultAge - 1),
            BiologicalSex.Female,
            Address.Create(
                "Sweet Apple Acres",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        return new()
        {
            DuplicateCandidates = [],
            GuardianPatient = guardianPatient,
            GuardianSelfMembership = FamilyMembership.CreateSelf(
                guardianPatient.Id,
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            ActiveFamilyMemberCount = 2,
            ReceptionistUserId = Guid.CreateVersion7(),
            ReferenceTime = _fakeTime.GetUtcNow().UtcDateTime,
        };
    }

    private PatientDuplicateCandidate CreateMatchingCandidate() =>
        new()
        {
            PatientId = Guid.CreateVersion7(),
            FullName = PersonName.Create("Apple", null, "Bloom"),
            DateOfBirth = DateOnly
                .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
                .AddYears(-DomainRules.AdultAge + 1),
            SexAtBirth = BiologicalSex.Female,
            HomeAddress = Address.Create(
                "Sweet Apple Acres",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            SsnLast4Matches = true,
        };
}

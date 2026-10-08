using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Consent;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class MedicalRecordConsentGrantServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Grant_ShouldCreateGrant_WhenValidParameters()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var requesterMembership = FamilyMembership.CreateSelf(
            patient.Id,
            requesterUserId,
            referenceTime
        );
        var recipientMembership = CreateRecipientMembership(patient.Id);
        var medicalRecord = CreateProtectedRecord(patient.Id);
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = requesterMembership,
            MedicalRecord = medicalRecord,
            RecipientMembership = recipientMembership,
            HasEffectiveGrant = false,
        };
        var args = new GrantMedicalRecordConsentArgs
        {
            RequesterUserId = requesterUserId,
            ReferenceTime = referenceTime,
        };

        // Act
        var grant = MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        grant.PatientId.Should().Be(patient.Id);
        grant.RecipientMembershipId.Should().Be(recipientMembership.Id);
        grant.MedicalRecordId.Should().Be(medicalRecord.Id);
        grant.SignedByUserId.Should().Be(requesterUserId);
        grant.SignedAt.Should().Be(referenceTime);
        grant.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void Grant_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var args = CreateValidArgs(Guid.CreateVersion7(), _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(null!, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Grant_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            MedicalRecord = CreateProtectedRecord(patient.Id),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            HasEffectiveGrant = false,
        };

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Grant_ShouldThrowArgumentNullException_WhenPatientIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = null!,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            MedicalRecord = CreateProtectedRecord(patient.Id),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            HasEffectiveGrant = false,
        };
        var args = CreateValidArgs(requesterUserId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Grant_ShouldThrowArgumentNullException_WhenRequesterMembershipIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = null!,
            MedicalRecord = CreateProtectedRecord(patient.Id),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            HasEffectiveGrant = false,
        };
        var args = CreateValidArgs(Guid.CreateVersion7(), _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Grant_ShouldThrowArgumentNullException_WhenMedicalRecordIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            MedicalRecord = null!,
            RecipientMembership = CreateRecipientMembership(patient.Id),
            HasEffectiveGrant = false,
        };

        var args = CreateValidArgs(requesterUserId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Grant_ShouldThrowArgumentNullException_WhenRecipientMembershipIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            MedicalRecord = CreateProtectedRecord(patient.Id),
            RecipientMembership = null!,
            HasEffectiveGrant = false,
        };

        var args = CreateValidArgs(requesterUserId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Grant_ShouldThrowDomainValidationException_WhenSignerIsNotSelf()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterMembership = CreateRecipientMembership(patient.Id); // Requester is a family member, not self
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = requesterMembership,
            MedicalRecord = CreateProtectedRecord(patient.Id),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            HasEffectiveGrant = false,
        };
        var args = CreateValidArgs(requesterMembership.UserId, referenceTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.UnauthorizedCreation);
    }

    [Fact]
    public void Grant_ShouldThrowDomainValidationException_WhenPatientIsExactlyAdultAge()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = Patient.CreateProfile(
            PersonName.Create("Jane Doe"),
            DateOnly
                .FromDateTime(_fakeTime.GetUtcNow().UtcDateTime)
                .AddYears(-DomainRules.AdultAge),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var requesterUserId = Guid.CreateVersion7();
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            MedicalRecord = CreateProtectedRecord(patient.Id),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            HasEffectiveGrant = false,
        };
        var args = CreateValidArgs(requesterUserId, referenceTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.PatientMustBeMinor);
    }

    [Fact]
    public void Grant_ShouldThrowDomainValidationException_WhenRecordPatientMismatches()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            MedicalRecord = CreateProtectedRecord(Guid.CreateVersion7()),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            HasEffectiveGrant = false,
        };
        var args = CreateValidArgs(requesterUserId, referenceTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.RecordPatientMismatch);
    }

    [Fact]
    public void Grant_ShouldThrowDomainValidationException_WhenRecordHasNoProtectedCategory()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var record = MedicalRecord.Create(
            patient.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "General checkup",
            protectedCareCategory: null,
            null
        );

        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            MedicalRecord = record,
            RecipientMembership = CreateRecipientMembership(patient.Id),
            HasEffectiveGrant = false,
        };
        var args = CreateValidArgs(requesterUserId, referenceTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.RecordMustHaveProtectedCategory);
    }

    [Fact]
    public void Grant_ShouldThrowDomainValidationException_WhenCategoryIsNotGrantableForAge()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = Patient.CreateProfile(
            PersonName.Create("Jane Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime).AddYears(-10),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var requesterUserId = Guid.CreateVersion7();
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            MedicalRecord = CreateProtectedRecord(patient.Id), // the category is MentalHealthCounseling, which is not grantable for a 10 year-old
            RecipientMembership = CreateRecipientMembership(patient.Id),
            HasEffectiveGrant = false,
        };
        var args = CreateValidArgs(requesterUserId, referenceTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.CategoryNotGrantable);
    }

    [Fact]
    public void Grant_ShouldThrowDomainValidationException_WhenRecipientIsSelf()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            MedicalRecord = CreateProtectedRecord(patient.Id),
            RecipientMembership = FamilyMembership.CreateSelf(
                patient.Id,
                Guid.CreateVersion7(),
                referenceTime
            ),
            HasEffectiveGrant = false,
        };
        var args = CreateValidArgs(requesterUserId, referenceTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.RecipientCannotBeSelf);
    }

    [Fact]
    public void Grant_ShouldThrowDomainValidationException_WhenEffectiveGrantAlreadyExists()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new MedicalRecordConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            MedicalRecord = CreateProtectedRecord(patient.Id),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            HasEffectiveGrant = true,
        };
        var args = CreateValidArgs(requesterUserId, referenceTime);

        // Act
        var act = () => MedicalRecordConsentGrantService.Grant(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.AlreadyExists);
    }

    private static GrantMedicalRecordConsentArgs CreateValidArgs(
        Guid requesterUserId,
        DateTime referenceTime
    ) => new() { RequesterUserId = requesterUserId, ReferenceTime = referenceTime };

    private Patient CreateMinorPatient() =>
        Patient.CreateProfile(
            PersonName.Create("Crazy"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime).AddYears(-15),
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private FamilyMembership CreateRecipientMembership(Guid patientId) =>
        FamilyMembership.CreateFamilyMember(
            patientId,
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private static MedicalRecord CreateProtectedRecord(Guid patientId) =>
        MedicalRecord.Create(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "General",
            ProtectedCategory.MentalHealthCounseling,
            null
        );
}

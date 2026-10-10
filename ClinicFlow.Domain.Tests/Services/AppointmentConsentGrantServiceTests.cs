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

public class AppointmentConsentGrantServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void GrantCategory_ShouldCreateGrant_WhenValidParameters()
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
        var category = ProtectedCategory.MentalHealthCounseling;
        var context = new AppointmentCategoryConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = requesterMembership,
            RecipientMembership = recipientMembership,
            ProtectedCareCategory = category,
            HasEffectiveGrant = false,
        };
        var args = new GrantAppointmentCategoryConsentArgs
        {
            RequesterUserId = requesterUserId,
            ReferenceTime = referenceTime,
        };

        // Act
        var grant = AppointmentConsentGrantService.GrantCategory(context, args);

        // Assert
        grant.PatientId.Should().Be(patient.Id);
        grant.RecipientMembershipId.Should().Be(recipientMembership.Id);
        grant.Scope.Should().Be(AppointmentConsentScope.Category);
        grant.ProtectedCareCategory.Should().Be(category);
        grant.AppointmentId.Should().BeNull();
        grant.SignedByUserId.Should().Be(requesterUserId);
        grant.SignedAt.Should().Be(referenceTime);
        grant.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void GrantCategory_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var args = CreateValidCategoryArgs(
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => AppointmentConsentGrantService.GrantCategory(null!, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantCategory_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new AppointmentCategoryConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            ProtectedCareCategory = ProtectedCategory.MentalHealthCounseling,
            HasEffectiveGrant = false,
        };

        // Act
        var act = () => AppointmentConsentGrantService.GrantCategory(context, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantCategory_ShouldThrowArgumentNullException_WhenPatientIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new AppointmentCategoryConsentGrantContext
        {
            Patient = null!,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            ProtectedCareCategory = ProtectedCategory.MentalHealthCounseling,
            HasEffectiveGrant = false,
        };
        var args = CreateValidCategoryArgs(requesterUserId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantCategory(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantCategory_ShouldThrowArgumentNullException_WhenRequesterMembershipIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var context = new AppointmentCategoryConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = null!,
            RecipientMembership = CreateRecipientMembership(patient.Id),
            ProtectedCareCategory = ProtectedCategory.MentalHealthCounseling,
            HasEffectiveGrant = false,
        };
        var args = CreateValidCategoryArgs(
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => AppointmentConsentGrantService.GrantCategory(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantCategory_ShouldThrowArgumentNullException_WhenRecipientMembershipIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new AppointmentCategoryConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RecipientMembership = null!,
            ProtectedCareCategory = ProtectedCategory.MentalHealthCounseling,
            HasEffectiveGrant = false,
        };
        var args = CreateValidCategoryArgs(requesterUserId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantCategory(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantCategory_ShouldThrowDomainValidationException_WhenSignerIsNotSelf()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterMembership = CreateRecipientMembership(patient.Id);
        var context = new AppointmentCategoryConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = requesterMembership,
            RecipientMembership = CreateRecipientMembership(patient.Id),
            ProtectedCareCategory = ProtectedCategory.MentalHealthCounseling,
            HasEffectiveGrant = false,
        };
        var args = CreateValidCategoryArgs(requesterMembership.UserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantCategory(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.UnauthorizedCreation);
    }

    [Fact]
    public void GrantCategory_ShouldThrowDomainValidationException_WhenPatientIsExactlyAdultAge()
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
        var context = new AppointmentCategoryConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            ProtectedCareCategory = ProtectedCategory.MentalHealthCounseling,
            HasEffectiveGrant = false,
        };
        var args = CreateValidCategoryArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantCategory(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.PatientMustBeMinor);
    }

    [Fact]
    public void GrantCategory_ShouldThrowDomainValidationException_WhenCategoryIsNotGrantableForAge()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = Patient.CreateProfile(
            PersonName.Create("Jane Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime).AddYears(-10),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var requesterUserId = Guid.CreateVersion7();
        var context = new AppointmentCategoryConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            ProtectedCareCategory = ProtectedCategory.MentalHealthCounseling,
            HasEffectiveGrant = false,
        };

        var args = CreateValidCategoryArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantCategory(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.CategoryNotGrantable);
    }

    [Fact]
    public void GrantCategory_ShouldThrowDomainValidationException_WhenRecipientIsSelf()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new AppointmentCategoryConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = FamilyMembership.CreateSelf(
                patient.Id,
                Guid.CreateVersion7(),
                referenceTime
            ),
            ProtectedCareCategory = ProtectedCategory.MentalHealthCounseling,
            HasEffectiveGrant = false,
        };
        var args = CreateValidCategoryArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantCategory(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.RecipientCannotBeSelf);
    }

    [Fact]
    public void GrantCategory_ShouldThrowDomainValidationException_WhenEffectiveGrantAlreadyExists()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new AppointmentCategoryConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            ProtectedCareCategory = ProtectedCategory.MentalHealthCounseling,
            HasEffectiveGrant = true,
        };
        var args = CreateValidCategoryArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantCategory(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.AlreadyExists);
    }

    [Fact]
    public void GrantAppointment_ShouldCreateGrant_WhenValidParameters()
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
        var appointmentType = CreateProtectedAppointmentType();

        var appointment = CreateAppointment(patient.Id, appointmentType.Id);
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = requesterMembership,
            RecipientMembership = recipientMembership,
            Appointment = appointment,
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };
        var args = new GrantSpecificAppointmentConsentArgs
        {
            RequesterUserId = requesterUserId,
            ReferenceTime = referenceTime,
        };

        // Act
        var grant = AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        grant.PatientId.Should().Be(patient.Id);
        grant.RecipientMembershipId.Should().Be(recipientMembership.Id);
        grant.Scope.Should().Be(AppointmentConsentScope.SpecificAppointment);
        grant.AppointmentId.Should().Be(appointment.Id);
        grant.ProtectedCareCategory.Should().BeNull();
        grant.SignedByUserId.Should().Be(requesterUserId);
        grant.SignedAt.Should().Be(referenceTime);
        grant.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void GrantAppointment_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var args = CreateValidAppointmentArgs(
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(null!, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantAppointment_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var appointmentType = CreateProtectedAppointmentType();
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantAppointment_ShouldThrowArgumentNullException_WhenPatientIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var appointmentType = CreateProtectedAppointmentType();

        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = null!,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };

        var args = CreateValidAppointmentArgs(requesterUserId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantAppointment_ShouldThrowArgumentNullException_WhenRequesterMembershipIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var appointmentType = CreateProtectedAppointmentType();
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = null!,
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };
        var args = CreateValidAppointmentArgs(
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantAppointment_ShouldThrowArgumentNullException_WhenRecipientMembershipIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var appointmentType = CreateProtectedAppointmentType();
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RecipientMembership = null!,
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };

        var args = CreateValidAppointmentArgs(requesterUserId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantAppointment_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = null!,
            AppointmentType = CreateProtectedAppointmentType(),
            HasEffectiveGrant = false,
        };
        var args = CreateValidAppointmentArgs(requesterUserId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantAppointment_ShouldThrowArgumentNullException_WhenAppointmentTypeIsNull()
    {
        // Arrange
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var appointmentType = CreateProtectedAppointmentType();
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = null!,
            HasEffectiveGrant = false,
        };
        var args = CreateValidAppointmentArgs(requesterUserId, _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GrantAppointment_ShouldThrowDomainValidationException_WhenSignerIsNotSelf()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterMembership = CreateRecipientMembership(patient.Id);
        var appointmentType = CreateProtectedAppointmentType();

        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = requesterMembership,
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };
        var args = CreateValidAppointmentArgs(requesterMembership.UserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.UnauthorizedCreation);
    }

    [Fact]
    public void GrantAppointment_ShouldThrowDomainValidationException_WhenPatientIsExactlyAdultAge()
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
        var appointmentType = CreateProtectedAppointmentType();
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };
        var args = CreateValidAppointmentArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.PatientMustBeMinor);
    }

    [Fact]
    public void GrantAppointment_ShouldThrowDomainValidationException_WhenAppointmentPatientMismatches()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var appointmentType = CreateProtectedAppointmentType();
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(Guid.CreateVersion7(), appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };

        var args = CreateValidAppointmentArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.AppointmentPatientMismatch);
    }

    [Fact]
    public void GrantAppointment_ShouldThrowDomainValidationException_WhenAppointmentTypeMismatches()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var appointmentType = CreateProtectedAppointmentType();
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(patient.Id, Guid.CreateVersion7()),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };
        var args = CreateValidAppointmentArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.AppointmentPatientMismatch);
    }

    [Fact]
    public void GrantAppointment_ShouldThrowDomainValidationException_WhenAppointmentTypeHasNoProtectedCategory()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            null,
            protectedCareCategory: null
        );

        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };
        var args = CreateValidAppointmentArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.AppointmentNotGrantable);
    }

    [Fact]
    public void GrantAppointment_ShouldThrowDomainValidationException_WhenCategoryIsNotGrantableForAge()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = Patient.CreateProfile(
            PersonName.Create("Jane Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime).AddYears(-10),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var requesterUserId = Guid.CreateVersion7();
        var appointmentType = CreateProtectedAppointmentType(); // Protected category is MentalHealthCounseling, which is not grantable for a 10 year old patient.
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };
        var args = CreateValidAppointmentArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.AppointmentNotGrantable);
    }

    [Fact]
    public void GrantAppointment_ShouldThrowDomainValidationException_WhenRecipientIsSelf()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var appointmentType = CreateProtectedAppointmentType();
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = FamilyMembership.CreateSelf(
                patient.Id,
                Guid.CreateVersion7(),
                referenceTime
            ),
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = false,
        };
        var args = CreateValidAppointmentArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.RecipientCannotBeSelf);
    }

    [Fact]
    public void GrantAppointment_ShouldThrowDomainValidationException_WhenEffectiveGrantAlreadyExists()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var patient = CreateMinorPatient();
        var requesterUserId = Guid.CreateVersion7();
        var appointmentType = CreateProtectedAppointmentType();
        var context = new SpecificAppointmentConsentGrantContext
        {
            Patient = patient,
            RequesterMembership = FamilyMembership.CreateSelf(
                patient.Id,
                requesterUserId,
                referenceTime
            ),
            RecipientMembership = CreateRecipientMembership(patient.Id),
            Appointment = CreateAppointment(patient.Id, appointmentType.Id),
            AppointmentType = appointmentType,
            HasEffectiveGrant = true,
        };
        var args = CreateValidAppointmentArgs(requesterUserId, referenceTime);

        // Act
        var act = () => AppointmentConsentGrantService.GrantAppointment(context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.AlreadyExists);
    }

    private static GrantAppointmentCategoryConsentArgs CreateValidCategoryArgs(
        Guid requesterUserId,
        DateTime referenceTime
    ) => new() { RequesterUserId = requesterUserId, ReferenceTime = referenceTime };

    private static GrantSpecificAppointmentConsentArgs CreateValidAppointmentArgs(
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

    private static AppointmentTypeDefinition CreateProtectedAppointmentType() =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            null,
            ProtectedCategory.MentalHealthCounseling
        );

    private Appointment CreateAppointment(Guid patientId, Guid appointmentTypeId) =>
        Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            appointmentTypeId,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
            Guid.CreateVersion7()
        );
}

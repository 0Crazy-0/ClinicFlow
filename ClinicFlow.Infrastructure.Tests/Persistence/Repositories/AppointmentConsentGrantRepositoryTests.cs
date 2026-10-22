using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class AppointmentConsentGrantRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly AppointmentConsentGrantRepository _sut = new(fixture.Context);
    private ApplicationDbContext Context => fixture.Context;

    public async ValueTask InitializeAsync()
    {
        await fixture.Respawner.ResetAsync(fixture.DbConnection);

        fixture.Context.ChangeTracker.Clear();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddGrantToContext()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var signer = await CreateUserAsync();
        var recipient = await CreateRecipientMembershipAsync(patient.Id);
        var signedAt = _fakeTime.GetLocalNow().DateTime;

        var grant = AppointmentConsentGrant.CreateForCategory(
            patient.Id,
            recipient.Id,
            ProtectedCategory.MentalHealthCounseling,
            signer.Id,
            signedAt
        );

        // Act
        await _sut.CreateAsync(grant, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .AppointmentConsentGrants.AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == grant.Id, TestContext.Current.CancellationToken);

        dbResult.Should().BeEquivalentTo(grant);
    }

    [Fact]
    public async Task CreateAsync_ShouldNotReAdd_WhenEntityIsAlreadyTracked()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var signer = await CreateUserAsync();
        var recipient = await CreateRecipientMembershipAsync(patient.Id);

        var grant = AppointmentConsentGrant.CreateForCategory(
            patient.Id,
            recipient.Id,
            ProtectedCategory.MentalHealthCounseling,
            signer.Id,
            _fakeTime.GetLocalNow().DateTime
        );

        Context.AppointmentConsentGrants.Add(grant);
        Context.Entry(grant).State = EntityState.Unchanged;

        // Act
        await _sut.CreateAsync(grant, TestContext.Current.CancellationToken);

        // Assert
        Context.Entry(grant).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnGrant_WhenExists()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();

        // Act
        var result = await _sut.GetByIdAsync(grant.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(grant);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HasEffectiveCategoryGrantAsync_ShouldReturnTrue_WhenEffectiveGrantExists()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync(); // Category is MentalHealthCounseling
        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var result = await _sut.HasEffectiveCategoryGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            ProtectedCategory.MentalHealthCounseling,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasEffectiveCategoryGrantAsync_ShouldReturnFalse_WhenPatientIdDoesNotMatch()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);
        var otherPatientId = Guid.CreateVersion7();

        // Act
        var result = await _sut.HasEffectiveCategoryGrantAsync(
            otherPatientId,
            grant.RecipientMembershipId,
            ProtectedCategory.MentalHealthCounseling,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveCategoryGrantAsync_ShouldReturnFalse_WhenRecipientMembershipIdDoesNotMatch()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);
        var otherRecipientMembershipId = Guid.CreateVersion7();

        // Act
        var result = await _sut.HasEffectiveCategoryGrantAsync(
            grant.PatientId,
            otherRecipientMembershipId,
            ProtectedCategory.MentalHealthCounseling,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveCategoryGrantAsync_ShouldReturnFalse_WhenScopeIsNotCategory()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var result = await _sut.HasEffectiveCategoryGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            ProtectedCategory.MentalHealthCounseling,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveCategoryGrantAsync_ShouldReturnFalse_WhenCategoryDoesNotMatch()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync(); // protected category is MentalHealthCounseling

        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var result = await _sut.HasEffectiveCategoryGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            ProtectedCategory.PregnancyPrevention,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveCategoryGrantAsync_ShouldReturnFalse_WhenGrantIsRevoked()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();

        var revokeTime = grant.SignedAt.AddDays(10);

        grant.Revoke(requesterIsAuthorized: true, referenceTime: revokeTime);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasEffectiveCategoryGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            ProtectedCategory.MentalHealthCounseling,
            revokeTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveCategoryGrantAsync_ShouldReturnFalse_WhenGrantIsExpired()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();
        var referenceTime = grant.ExpiresAt.AddDays(1);

        // Act
        var result = await _sut.HasEffectiveCategoryGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            ProtectedCategory.MentalHealthCounseling,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveCategoryGrantAsync_ShouldReturnFalse_WhenReferenceTimeEqualsExpiry()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();

        // Act
        var result = await _sut.HasEffectiveCategoryGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            ProtectedCategory.MentalHealthCounseling,
            grant.ExpiresAt,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveAppointmentGrantAsync_ShouldReturnTrue_WhenEffectiveGrantExists()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var result = await _sut.HasEffectiveAppointmentGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            grant.AppointmentId!.Value,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasEffectiveAppointmentGrantAsync_ShouldReturnFalse_WhenPatientIdDoesNotMatch()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);
        var otherPatientId = Guid.CreateVersion7();

        // Act
        var result = await _sut.HasEffectiveAppointmentGrantAsync(
            otherPatientId,
            grant.RecipientMembershipId,
            grant.AppointmentId!.Value,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveAppointmentGrantAsync_ShouldReturnFalse_WhenRecipientMembershipIdDoesNotMatch()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);
        var otherRecipientMembershipId = Guid.CreateVersion7();

        // Act
        var result = await _sut.HasEffectiveAppointmentGrantAsync(
            grant.PatientId,
            otherRecipientMembershipId,
            grant.AppointmentId!.Value,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveAppointmentGrantAsync_ShouldReturnFalse_WhenScopeIsNotSpecificAppointment()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);
        var otherAppointmentId = Guid.CreateVersion7();

        // Act
        var result = await _sut.HasEffectiveAppointmentGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            otherAppointmentId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveAppointmentGrantAsync_ShouldReturnFalse_WhenAppointmentIdDoesNotMatch()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);
        var otherAppointmentId = Guid.CreateVersion7();

        // Act
        var result = await _sut.HasEffectiveAppointmentGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            otherAppointmentId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveAppointmentGrantAsync_ShouldReturnFalse_WhenGrantIsRevoked()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var revokeTime = grant.SignedAt.AddDays(10);

        grant.Revoke(requesterIsAuthorized: true, referenceTime: revokeTime);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasEffectiveAppointmentGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            grant.AppointmentId!.Value,
            revokeTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveAppointmentGrantAsync_ShouldReturnFalse_WhenGrantIsExpired()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var referenceTime = grant.ExpiresAt.AddDays(1);

        // Act
        var result = await _sut.HasEffectiveAppointmentGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            grant.AppointmentId!.Value,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveAppointmentGrantAsync_ShouldReturnFalse_WhenReferenceTimeEqualsExpiry()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();

        // Act
        var result = await _sut.HasEffectiveAppointmentGrantAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            grant.AppointmentId!.Value,
            grant.ExpiresAt,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetEffectiveCategoriesAsync_ShouldReturnCategory_WhenEffectiveGrantExists()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var results = await _sut.GetEffectiveCategoriesAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(ProtectedCategory.MentalHealthCounseling);
    }

    [Fact]
    public async Task GetEffectiveCategoriesAsync_ShouldReturnOnlyMatchingPatientCategories()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync(
            category: ProtectedCategory.MentalHealthCounseling
        );

        await CreateCategoryGrantAsync(category: ProtectedCategory.SubstanceAbuseTreatment);

        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var results = await _sut.GetEffectiveCategoriesAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(ProtectedCategory.MentalHealthCounseling);
    }

    [Fact]
    public async Task GetEffectiveCategoriesAsync_ShouldReturnEmpty_WhenRecipientDoesNotMatch()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);
        var otherRecipientMembershipId = Guid.CreateVersion7();

        // Act
        var results = await _sut.GetEffectiveCategoriesAsync(
            grant.PatientId,
            otherRecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveCategoriesAsync_ShouldReturnEmpty_WhenOnlyAppointmentScopeGrantExists()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var results = await _sut.GetEffectiveCategoriesAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveCategoriesAsync_ShouldExcludeRevokedGrants()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();
        var revokeTime = grant.SignedAt.AddDays(10);

        grant.Revoke(requesterIsAuthorized: true, referenceTime: revokeTime);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetEffectiveCategoriesAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            revokeTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveCategoriesAsync_ShouldExcludeExpiredGrants()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();
        var referenceTime = grant.ExpiresAt.AddDays(1);

        // Act
        var results = await _sut.GetEffectiveCategoriesAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveCategoriesAsync_ShouldExcludeGrant_WhenReferenceTimeEqualsExpiry()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();

        // Act
        var results = await _sut.GetEffectiveCategoriesAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            grant.ExpiresAt,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveCategoriesAsync_ShouldReturnEmpty_WhenNoGrantsExist()
    {
        // Arrange
        var nonExistentPatientId = Guid.CreateVersion7();
        var nonExistentRecipientId = Guid.CreateVersion7();
        var referenceTime = _fakeTime.GetLocalNow().DateTime;

        // Act
        var results = await _sut.GetEffectiveCategoriesAsync(
            nonExistentPatientId,
            nonExistentRecipientId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveAppointmentIdsAsync_ShouldReturnAppointmentId_WhenEffectiveGrantExists()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var results = await _sut.GetEffectiveAppointmentIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().ContainSingle().Which.Should().Be(grant.AppointmentId!.Value);
    }

    [Fact]
    public async Task GetEffectiveAppointmentIdsAsync_ShouldReturnOnlyMatchingPatientAppointmentIds()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();

        await CreateAppointmentGrantAsync();

        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var results = await _sut.GetEffectiveAppointmentIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().ContainSingle().Which.Should().Be(grant.AppointmentId!.Value);
    }

    [Fact]
    public async Task GetEffectiveAppointmentIdsAsync_ShouldReturnEmpty_WhenRecipientDoesNotMatch()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);
        var otherRecipientMembershipId = Guid.CreateVersion7();

        // Act
        var results = await _sut.GetEffectiveAppointmentIdsAsync(
            grant.PatientId,
            otherRecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveAppointmentIdsAsync_ShouldReturnEmpty_WhenOnlyCategoryScopeGrantExists()
    {
        // Arrange
        var grant = await CreateCategoryGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var results = await _sut.GetEffectiveAppointmentIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveAppointmentIdsAsync_ShouldExcludeRevokedGrants()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var revokeTime = grant.SignedAt.AddDays(10);

        grant.Revoke(requesterIsAuthorized: true, referenceTime: revokeTime);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetEffectiveAppointmentIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            revokeTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveAppointmentIdsAsync_ShouldExcludeExpiredGrants()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();
        var referenceTime = grant.ExpiresAt.AddDays(1);

        // Act
        var results = await _sut.GetEffectiveAppointmentIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveAppointmentIdsAsync_ShouldExcludeGrant_WhenReferenceTimeEqualsExpiry()
    {
        // Arrange
        var grant = await CreateAppointmentGrantAsync();

        // Act
        var results = await _sut.GetEffectiveAppointmentIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            grant.ExpiresAt,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveAppointmentIdsAsync_ShouldReturnEmpty_WhenNoGrantsExist()
    {
        // Arrange
        var nonExistentPatientId = Guid.CreateVersion7();
        var nonExistentRecipientId = Guid.CreateVersion7();
        var referenceTime = _fakeTime.GetLocalNow().DateTime;

        // Act
        var results = await _sut.GetEffectiveAppointmentIdsAsync(
            nonExistentPatientId,
            nonExistentRecipientId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    private async Task<User> CreateUserAsync()
    {
        var user = User.Create(
            EmailAddress.Create($"{Guid.CreateVersion7()}@clinic.com"),
            "password",
            PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}"),
            UserRole.Patient
        );

        Context.Users.Add(user);

        await Context.SaveChangesAsync();

        return user;
    }

    private async Task<Patient> CreatePatientAsync()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetLocalNow().DateTime.AddYears(-30)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetLocalNow().DateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Contact", "555-9999"));

        Context.Patients.Add(patient);

        await Context.SaveChangesAsync();

        return patient;
    }

    private async Task<FamilyMembership> CreateRecipientMembershipAsync(Guid patientId)
    {
        var user = await CreateUserAsync();

        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            user.Id,
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetLocalNow().DateTime
        );

        Context.FamilyMemberships.Add(membership);

        await Context.SaveChangesAsync();

        return membership;
    }

    private async Task<AppointmentConsentGrant> CreateCategoryGrantAsync(
        ProtectedCategory category = ProtectedCategory.MentalHealthCounseling
    )
    {
        var patient = await CreatePatientAsync();
        var signer = await CreateUserAsync();
        var recipient = await CreateRecipientMembershipAsync(patient.Id);
        var signedAt = _fakeTime.GetLocalNow().DateTime;

        var grant = AppointmentConsentGrant.CreateForCategory(
            patient.Id,
            recipient.Id,
            category,
            signer.Id,
            signedAt
        );

        Context.AppointmentConsentGrants.Add(grant);

        await Context.SaveChangesAsync();

        return grant;
    }

    private async Task<AppointmentConsentGrant> CreateAppointmentGrantAsync()
    {
        var patient = await CreatePatientAsync();
        var signer = await CreateUserAsync();
        var recipient = await CreateRecipientMembershipAsync(patient.Id);

        var doctorUser = await CreateUserAsync();
        var specialty = MedicalSpecialty.Create("Cardiology", "Desc", 30, 24);

        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync();

        var roomNumber = Random.Shared.Next(
            ConsultationRoom.MinimumNumber,
            ConsultationRoom.MaximumNumber + 1
        );

        var doctor = Doctor.Create(
            doctorUser.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("LicenseNumber"),
            specialty.Id,
            "Desc",
            ConsultationRoom.Create(
                roomNumber,
                $"Room {roomNumber}",
                Random.Shared.Next(ConsultationRoom.MinimumFloor, ConsultationRoom.MaximumFloor + 1)
            )
        );

        Context.Doctors.Add(doctor);

        await Context.SaveChangesAsync();

        var apptType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            AppointmentPurpose.FirstConsultation,
            $"Consultation-{Guid.CreateVersion7():N}",
            "Desc",
            EncounterDuration.FromMinutes(20)
        );

        Context.AppointmentTypes.Add(apptType);

        await Context.SaveChangesAsync();

        var startMinute = Random.Shared.Next(0, 480);

        var appointment = Appointment.Schedule(
            patient.Id,
            doctor.Id,
            apptType.Id,
            DateOnly.FromDateTime(_fakeTime.GetLocalNow().DateTime.AddDays(1)),
            TimeRange.Create(
                new TimeOnly(8, 0).AddMinutes(startMinute),
                new TimeOnly(8, 0).AddMinutes(startMinute + 30)
            ),
            Guid.CreateVersion7()
        );

        Context.Appointments.Add(appointment);

        await Context.SaveChangesAsync();

        var signedAt = _fakeTime.GetLocalNow().DateTime;

        var grant = AppointmentConsentGrant.CreateForAppointment(
            patient.Id,
            recipient.Id,
            appointment.Id,
            signer.Id,
            signedAt
        );

        Context.AppointmentConsentGrants.Add(grant);

        await Context.SaveChangesAsync();

        return grant;
    }
}

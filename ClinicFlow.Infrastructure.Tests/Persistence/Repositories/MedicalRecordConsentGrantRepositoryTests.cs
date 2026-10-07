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

public class MedicalRecordConsentGrantRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly MedicalRecordConsentGrantRepository _sut = new(fixture.Context);
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

        var record = await CreateMedicalRecordAsync(patient.Id);
        var signedAt = _fakeTime.GetLocalNow().DateTime;

        var grant = MedicalRecordConsentGrant.Create(
            patient.Id,
            recipient.Id,
            record.Id,
            signer.Id,
            signedAt
        );

        // Act
        await _sut.CreateAsync(grant, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .MedicalRecordConsentGrants.AsNoTracking()
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
        var record = await CreateMedicalRecordAsync(patient.Id);

        var grant = MedicalRecordConsentGrant.Create(
            patient.Id,
            recipient.Id,
            record.Id,
            signer.Id,
            _fakeTime.GetLocalNow().DateTime
        );

        Context.MedicalRecordConsentGrants.Add(grant);
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
        var grant = await CreateGrantAsync();

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
    public async Task HasEffectiveGrantAsync_ShouldReturnTrue_WhenEffectiveGrantExists()
    {
        // Arrange
        var grant = await CreateGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var result = await _sut.HasEffectiveGrantAsync(
            grant.MedicalRecordId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasEffectiveGrantAsync_ShouldReturnFalse_WhenMedicalRecordIdDoesNotMatch()
    {
        // Arrange
        var grant = await CreateGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        var otherMedicalRecordId = Guid.CreateVersion7();

        // Act
        var result = await _sut.HasEffectiveGrantAsync(
            otherMedicalRecordId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveGrantAsync_ShouldReturnFalse_WhenRecipientMembershipIdDoesNotMatch()
    {
        // Arrange
        var grant = await CreateGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        var otherRecipientMembershipId = Guid.CreateVersion7();

        // Act
        var result = await _sut.HasEffectiveGrantAsync(
            grant.MedicalRecordId,
            otherRecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveGrantAsync_ShouldReturnFalse_WhenGrantIsRevoked()
    {
        // Arrange
        var grant = await CreateGrantAsync();
        var revokeTime = grant.SignedAt.AddDays(10);

        grant.Revoke(requesterIsAuthorized: true, referenceTime: revokeTime);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasEffectiveGrantAsync(
            grant.MedicalRecordId,
            grant.RecipientMembershipId,
            revokeTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveGrantAsync_ShouldReturnFalse_WhenGrantIsExpired()
    {
        // Arrange
        var grant = await CreateGrantAsync();
        var referenceTime = grant.ExpiresAt.AddDays(1);

        // Act
        var result = await _sut.HasEffectiveGrantAsync(
            grant.MedicalRecordId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasEffectiveGrantAsync_ShouldReturnFalse_WhenReferenceTimeEqualsExpiry()
    {
        // Arrange
        var grant = await CreateGrantAsync();

        // Act
        var result = await _sut.HasEffectiveGrantAsync(
            grant.MedicalRecordId,
            grant.RecipientMembershipId,
            grant.ExpiresAt, // ExpiresAt is always SignedAt + ValidityYears, never null
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetEffectiveRecordIdsAsync_ShouldReturnRecordId_WhenEffectiveGrantExists()
    {
        // Arrange
        var grant = await CreateGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var results = await _sut.GetEffectiveRecordIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().ContainSingle().Which.Should().Be(grant.MedicalRecordId);
    }

    [Fact]
    public async Task GetEffectiveRecordIdsAsync_ShouldReturnOnlyMatchingPatientRecords()
    {
        // Arrange
        var grant = await CreateGrantAsync();

        await CreateGrantAsync();

        var referenceTime = grant.SignedAt.AddDays(10);

        // Act
        var results = await _sut.GetEffectiveRecordIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().ContainSingle().Which.Should().Be(grant.MedicalRecordId);
    }

    [Fact]
    public async Task GetEffectiveRecordIdsAsync_ShouldReturnEmpty_WhenRecipientDoesNotMatch()
    {
        // Arrange
        var grant = await CreateGrantAsync();
        var referenceTime = grant.SignedAt.AddDays(10);

        var otherRecipientMembershipId = Guid.CreateVersion7();

        // Act
        var results = await _sut.GetEffectiveRecordIdsAsync(
            grant.PatientId,
            otherRecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveRecordIdsAsync_ShouldExcludeRevokedGrants()
    {
        // Arrange
        var grant = await CreateGrantAsync();
        var revokeTime = grant.SignedAt.AddDays(10);

        grant.Revoke(requesterIsAuthorized: true, referenceTime: revokeTime);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetEffectiveRecordIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            revokeTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveRecordIdsAsync_ShouldExcludeExpiredGrants()
    {
        // Arrange
        var grant = await CreateGrantAsync();
        var referenceTime = grant.ExpiresAt.AddDays(1);

        // Act
        var results = await _sut.GetEffectiveRecordIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveRecordIdsAsync_ShouldExcludeGrant_WhenReferenceTimeEqualsExpiry()
    {
        // Arrange
        var grant = await CreateGrantAsync();

        // Act
        var results = await _sut.GetEffectiveRecordIdsAsync(
            grant.PatientId,
            grant.RecipientMembershipId,
            grant.ExpiresAt, // ExpiresAt is always SignedAt + ValidityYears, never null
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEffectiveRecordIdsAsync_ShouldReturnEmpty_WhenNoGrantsExist()
    {
        // Arrange
        var nonExistentPatientId = Guid.CreateVersion7();
        var nonExistentRecipientId = Guid.CreateVersion7();
        var referenceTime = _fakeTime.GetLocalNow().DateTime;

        // Act
        var results = await _sut.GetEffectiveRecordIdsAsync(
            nonExistentPatientId,
            nonExistentRecipientId,
            referenceTime,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnPaginatedGrants_ForPatient()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var signer = await CreateUserAsync();
        var recipient = await CreateRecipientMembershipAsync(patient.Id);
        var signedAt = _fakeTime.GetLocalNow().DateTime;

        await CreateGrantAsync(patient.Id, recipient.Id, signedAt, signer.Id);

        var grant2 = await CreateGrantAsync(patient.Id, recipient.Id, signedAt, signer.Id);
        var grant3 = await CreateGrantAsync(patient.Id, recipient.Id, signedAt, signer.Id);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedAsync(
            patient.Id,
            pageNumber: 1,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().BeEquivalentTo([grant3, grant2], options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnSecondPage()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var signer = await CreateUserAsync();
        var recipient = await CreateRecipientMembershipAsync(patient.Id);
        var signedAt = _fakeTime.GetLocalNow().DateTime;

        var grant1 = await CreateGrantAsync(patient.Id, recipient.Id, signedAt, signer.Id);

        await CreateGrantAsync(patient.Id, recipient.Id, signedAt, signer.Id);
        await CreateGrantAsync(patient.Id, recipient.Id, signedAt, signer.Id);

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedAsync(
            patient.Id,
            pageNumber: 2,
            pageSize: 2,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(3);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(grant1);
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnOnlyGrantsFromRequestedPatient()
    {
        // Arrange
        var patient1 = await CreatePatientAsync();
        var signer1 = await CreateUserAsync();
        var recipient1 = await CreateRecipientMembershipAsync(patient1.Id);

        var grant1 = await CreateGrantAsync(
            patient1.Id,
            recipient1.Id,
            _fakeTime.GetLocalNow().DateTime,
            signer1.Id
        );

        var patient2 = await CreatePatientAsync();
        var signer2 = await CreateUserAsync();
        var recipient2 = await CreateRecipientMembershipAsync(patient2.Id);

        await CreateGrantAsync(
            patient2.Id,
            recipient2.Id,
            _fakeTime.GetLocalNow().DateTime,
            signer2.Id
        );

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedAsync(
            patient1.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(1);

        items.Should().ContainSingle().Which.Should().BeEquivalentTo(grant1);
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnEmpty_WhenNoGrantsForPatient()
    {
        // Arrange
        var nonExistentPatientId = Guid.CreateVersion7();

        // Act
        var (items, totalCount) = await _sut.GetByPatientIdPaginatedAsync(
            nonExistentPatientId,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        totalCount.Should().Be(0);

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByPatientIdPaginatedAsync_ShouldReturnGrantsOrderedBySequenceNumberDescending()
    {
        // Arrange
        var patient = await CreatePatientAsync();
        var signer = await CreateUserAsync();
        var recipient = await CreateRecipientMembershipAsync(patient.Id);
        var signedAt = _fakeTime.GetLocalNow().DateTime;

        var grant1 = await CreateGrantAsync(patient.Id, recipient.Id, signedAt, signer.Id);
        var grant2 = await CreateGrantAsync(patient.Id, recipient.Id, signedAt, signer.Id);
        var grant3 = await CreateGrantAsync(patient.Id, recipient.Id, signedAt, signer.Id);

        // Act
        var (items, _) = await _sut.GetByPatientIdPaginatedAsync(
            patient.Id,
            pageNumber: 1,
            pageSize: 10,
            TestContext.Current.CancellationToken
        );

        // Assert
        items
            .Should()
            .BeEquivalentTo([grant3, grant2, grant1], options => options.WithStrictOrdering());
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
            _fakeTime.GetLocalNow().DateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Contact", "555-9999"));

        Context.Patients.Add(patient);

        await Context.SaveChangesAsync();

        return patient;
    }

    private async Task<Appointment> CreateAppointmentAsync(Guid patientId, Guid doctorId)
    {
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
            patientId,
            doctorId,
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

        return appointment;
    }

    private async Task<MedicalRecord> CreateMedicalRecordAsync(Guid patientId)
    {
        var user = User.Create(
            EmailAddress.Create($"{Guid.CreateVersion7()}@clinic.com"),
            "password",
            PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}"),
            UserRole.Doctor
        );

        Context.Users.Add(user);

        await Context.SaveChangesAsync();

        var specialty = MedicalSpecialty.Create("Cardiology", "Desc", 30, 24);

        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync();

        var roomNumber = Random.Shared.Next(
            ConsultationRoom.MinimumNumber,
            ConsultationRoom.MaximumNumber + 1
        );

        var doctor = Doctor.Create(
            user.Id,
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

        var appointment = await CreateAppointmentAsync(patientId, doctor.Id);
        var record = MedicalRecord.Create(
            patientId,
            doctor.Id,
            appointment.Id,
            "chiefComplaint",
            null,
            null
        );

        Context.MedicalRecords.Add(record);

        await Context.SaveChangesAsync();

        return record;
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

    private async Task<MedicalRecordConsentGrant> CreateGrantAsync()
    {
        var patient = await CreatePatientAsync();
        var signer = await CreateUserAsync();
        var recipient = await CreateRecipientMembershipAsync(patient.Id);
        var record = await CreateMedicalRecordAsync(patient.Id);
        var signedAt = _fakeTime.GetLocalNow().DateTime;

        return await CreateGrantAsync(patient.Id, recipient.Id, signedAt, signer.Id, record.Id);
    }

    private async Task<MedicalRecordConsentGrant> CreateGrantAsync(
        Guid patientId,
        Guid recipientMembershipId,
        DateTime signedAt,
        Guid signedByUserId
    )
    {
        var record = await CreateMedicalRecordAsync(patientId);

        return await CreateGrantAsync(
            patientId,
            recipientMembershipId,
            signedAt,
            signedByUserId,
            record.Id
        );
    }

    private async Task<MedicalRecordConsentGrant> CreateGrantAsync(
        Guid patientId,
        Guid recipientMembershipId,
        DateTime signedAt,
        Guid signedByUserId,
        Guid medicalRecordId
    )
    {
        var grant = MedicalRecordConsentGrant.Create(
            patientId,
            recipientMembershipId,
            medicalRecordId,
            signedByUserId,
            signedAt
        );

        Context.MedicalRecordConsentGrants.Add(grant);

        await Context.SaveChangesAsync();

        return grant;
    }
}

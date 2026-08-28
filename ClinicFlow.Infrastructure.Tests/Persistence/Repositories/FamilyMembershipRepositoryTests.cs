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

public class FamilyMembershipRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly FamilyMembershipRepository _sut = new(fixture.Context);
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
    public async Task CreateAsync_ShouldAddFamilyMembershipToContext()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            user.Id,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        await _sut.CreateAsync(membership, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .FamilyMemberships.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == membership.Id, TestContext.Current.CancellationToken);

        dbResult.Should().BeEquivalentTo(membership);
    }

    [Fact]
    public async Task CreateAsync_ShouldNotReAdd_WhenEntityIsAlreadyTracked()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            user.Id,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        Context.Entry(membership).State = EntityState.Unchanged;

        // Act
        await _sut.CreateAsync(membership, TestContext.Current.CancellationToken);

        // Assert
        Context.Entry(membership).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task GetActiveSelfMembershipByUserIdAsync_ShouldReturnMembership_WhenExistsAndActive()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            user.Id,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveSelfMembershipByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(membership);
    }

    [Fact]
    public async Task GetActiveSelfMembershipByUserIdAsync_ShouldReturnNull_WhenMembershipIsTerminated()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            user.Id,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        membership.CloseSelfMembership(_fakeTime.GetUtcNow().UtcDateTime.AddHours(1));

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveSelfMembershipByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveSelfMembershipByUserIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetActiveSelfMembershipByUserIdAsync(
            nonExistentUserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveSelfMembershipByUserIdAsync_ShouldReturnNull_WhenUserHasOnlyFamilyMember()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();

        // in production, a FamilyMember membership always requires the owner
        // to already have an active Self membership (enforced by PrimaryPatientRequired
        // upstream). It's created here in isolation, without the corresponding Self,
        // solely to verify that the query correctly filters by Role == Self.
        var membership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            user.Id,
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveSelfMembershipByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveMembershipAsync_ShouldReturnMembership_WhenExistsAndActive()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            user.Id,
            PatientRelationship.Spouse,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveMembershipAsync(
            user.Id,
            patient.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(membership);
    }

    [Fact]
    public async Task GetActiveMembershipAsync_ShouldReturnNull_WhenUserIdDoesNotMatch()
    {
        // Arrange
        var ownerUser = await CreateUserAsync();
        var otherUser = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            ownerUser.Id,
            PatientRelationship.Spouse,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveMembershipAsync(
            otherUser.Id,
            patient.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveMembershipAsync_ShouldReturnNull_WhenPatientIdDoesNotMatch()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient1 = await CreatePatientAsync();
        var patient2 = await CreatePatientAsync();
        var membership = FamilyMembership.CreateFamilyMember(
            patient1.Id,
            user.Id,
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveMembershipAsync(
            user.Id,
            patient2.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveMembershipAsync_ShouldReturnNull_WhenMembershipIsTerminated()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            user.Id,
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        membership.Leave(
            FamilyMembership.MinimumAgeToLeave,
            _fakeTime.GetUtcNow().UtcDateTime.AddHours(1)
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetActiveMembershipAsync(
            user.Id,
            patient.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveMembershipAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.CreateVersion7();
        var nonExistentPatientId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetActiveMembershipAsync(
            nonExistentUserId,
            nonExistentPatientId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HasActiveSelfMembershipByUserIdAsync_ShouldReturnTrue_WhenActiveSelfMembershipExists()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            user.Id,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveSelfMembershipByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveSelfMembershipByUserIdAsync_ShouldReturnFalse_WhenMembershipIsClosed()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            user.Id,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        membership.CloseSelfMembership(_fakeTime.GetUtcNow().UtcDateTime.AddHours(1));

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveSelfMembershipByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveSelfMembershipByUserIdAsync_ShouldReturnFalse_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.CreateVersion7();

        // Act
        var result = await _sut.HasActiveSelfMembershipByUserIdAsync(
            nonExistentUserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveSelfMembershipByUserIdAsync_ShouldReturnFalse_WhenUserHasOnlyFamilyMember()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();

        // in production, a FamilyMember membership always requires the owner
        // to already have an active Self membership (enforced by PrimaryPatientRequired
        // upstream). It's created here in isolation, without the corresponding Self,
        // solely to verify that the query correctly filters by Role == Self.
        var membership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            user.Id,
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveSelfMembershipByUserIdAsync(
            user.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveSelfMembershipByPatientIdAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            user.Id,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveSelfMembershipByPatientIdAsync(
            patient.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveSelfMembershipByPatientIdAsync_ShouldReturnFalse_WhenMembershipIsClosed()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            user.Id,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        membership.CloseSelfMembership(_fakeTime.GetUtcNow().UtcDateTime.AddHours(1));

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveSelfMembershipByPatientIdAsync(
            patient.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveMembershipAsync_ShouldReturnTrue_WhenMatchesUserAndPatient()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            user.Id,
            PatientRelationship.Spouse,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveMembershipAsync(
            user.Id,
            patient.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveMembershipAsync_ShouldReturnFalse_WhenUserIdDoesNotMatch()
    {
        // Arrange
        var ownerUser = await CreateUserAsync();
        var otherUser = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            ownerUser.Id,
            PatientRelationship.Spouse,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveMembershipAsync(
            otherUser.Id,
            patient.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveMembershipAsync_ShouldReturnFalse_WhenMembershipIsTerminated()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var membership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            user.Id,
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        membership.Leave(
            FamilyMembership.MinimumAgeToLeave,
            _fakeTime.GetUtcNow().UtcDateTime.AddHours(1)
        );

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.HasActiveMembershipAsync(
            user.Id,
            patient.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountActiveFamilyMembersAsync_ShouldReturnCountExcludingSelf()
    {
        // Arrange
        var ownerUser = await CreateUserAsync();
        var otherUser = await CreateUserAsync();
        var patientSelf = await CreatePatientAsync();
        var patientChild = await CreatePatientAsync();
        var patientSpouse = await CreatePatientAsync();
        var patientOtherChild = await CreatePatientAsync();
        var now = _fakeTime.GetUtcNow().UtcDateTime;

        var selfMembership = FamilyMembership.CreateSelf(patientSelf.Id, ownerUser.Id, now);
        Context.FamilyMemberships.Add(selfMembership);

        var childMembership = FamilyMembership.CreateFamilyMember(
            patientChild.Id,
            ownerUser.Id,
            PatientRelationship.Child,
            now
        );
        Context.FamilyMemberships.Add(childMembership);

        var spouseMembership = FamilyMembership.CreateFamilyMember(
            patientSpouse.Id,
            ownerUser.Id,
            PatientRelationship.Spouse,
            now
        );
        Context.FamilyMemberships.Add(spouseMembership);

        var otherChildMembership = FamilyMembership.CreateFamilyMember(
            patientOtherChild.Id,
            otherUser.Id,
            PatientRelationship.Child,
            now
        );
        Context.FamilyMemberships.Add(otherChildMembership);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var count = await _sut.CountActiveFamilyMembersAsync(
            ownerUser.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetHistoryByPatientIdAsync_ShouldReturnAllMembershipsForPatient()
    {
        // Arrange
        var user1 = await CreateUserAsync();
        var user2 = await CreateUserAsync();
        var patient = await CreatePatientAsync();
        var startTime = _fakeTime.GetUtcNow().UtcDateTime;

        var oldMembership = FamilyMembership.CreateSelf(patient.Id, user1.Id, startTime);

        oldMembership.CloseSelfMembership(startTime.AddDays(1));

        Context.FamilyMemberships.Add(oldMembership);

        var newMembership = FamilyMembership.CreateSelf(patient.Id, user2.Id, startTime.AddDays(2));

        Context.FamilyMemberships.Add(newMembership);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var history = await _sut.GetHistoryByPatientIdAsync(
            patient.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        history
            .Should()
            .BeEquivalentTo(
                [newMembership, oldMembership],
                options => options.WithStrictOrdering()
            );
    }

    private async Task<User> CreateUserAsync()
    {
        var email = EmailAddress.Create($"{Guid.CreateVersion7()}@clinic.com");
        var phone = PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}");
        var user = User.Create(email, "password", phone, UserRole.Patient);

        Context.Users.Add(user);

        await Context.SaveChangesAsync();

        return user;
    }

    private async Task<Patient> CreatePatientAsync()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Patient"),
            new DateOnly(1990, 1, 1),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Contact", "555-9999"));

        Context.Patients.Add(patient);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return patient;
    }
}

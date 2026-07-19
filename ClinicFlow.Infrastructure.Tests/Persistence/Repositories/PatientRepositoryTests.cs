using System.Globalization;
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

public class PatientRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly PatientRepository _sut = new(fixture.Context);
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
    public async Task CreateAsync_ShouldAddPatientToContext()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = Patient.CreateSelf(
            user.Id,
            PersonName.Create("New Patient"),
            new DateOnly(1990, 1, 1),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Contact", "555-9999"));

        // Act
        await _sut.CreateAsync(patient, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .Patients.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patient.Id, TestContext.Current.CancellationToken);

        dbResult.Should().BeEquivalentTo(patient);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPatient_WhenExistsAndActive()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();

        // Act
        var result = await _sut.GetByIdAsync(patient.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(patient);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();

        patient.CloseAccount(false);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(patient.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
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
    public async Task GetSelfPatientByUserIdAsync_ShouldReturnPatient_WhenExistsAndActive()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();

        // Act
        var result = await _sut.GetSelfPatientByUserIdAsync(
            patient.UserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(patient);
    }

    [Fact]
    public async Task GetSelfPatientByUserIdAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();

        patient.CloseAccount(false);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetSelfPatientByUserIdAsync(
            patient.UserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSelfPatientByUserIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentUserId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetSelfPatientByUserIdAsync(
            nonExistentUserId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSelfPatientByUserIdAsync_ShouldReturnSelfPatient_WhenUserHasFamilyMemberToo()
    {
        // Arrange
        var user = await CreateUserAsync();
        var selfPatient = await CreateSelfPatientAsync(
            user.Id,
            "John Doe",
            new DateOnly(1990, 1, 1)
        );
        await CreateFamilyMemberPatientAsync(user.Id, "Family Member", new DateOnly(2015, 1, 1));

        // Act
        var result = await _sut.GetSelfPatientByUserIdAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(selfPatient);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ShouldReturnAllPatients_WhenUserHasMultiple()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient1 = await CreateSelfPatientAsync(
            user.Id,
            "Patient One",
            new DateOnly(1990, 1, 1)
        );
        var patient2 = await CreateFamilyMemberPatientAsync(
            user.Id,
            "Patient Two",
            new DateOnly(2020, 1, 1)
        );

        // Act
        var result = await _sut.GetAllByUserIdAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo([patient1, patient2]);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ShouldReturnEmpty_WhenUserHasNoPatients()
    {
        // Arrange
        var user = await CreateUserAsync();

        // Act
        var result = await _sut.GetAllByUserIdAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ShouldExcludeSoftDeleted()
    {
        // Arrange
        var user = await CreateUserAsync();
        var active = await CreateSelfPatientAsync(
            user.Id,
            "Active Patient",
            new DateOnly(1990, 1, 1)
        );

        var deleted = await CreateFamilyMemberPatientAsync(
            user.Id,
            "Deleted Patient",
            new DateOnly(2020, 1, 1),
            "B+"
        );

        deleted.RemoveFamilyMember(deleted.UserId);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetAllByUserIdAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().ContainSingle().Which.Should().BeEquivalentTo(active);
    }

    [Fact]
    public async Task GetIncludingDeletedByNameAndDobAsync_ShouldReturnPatient_WhenExistsAndActive()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();

        // Act
        var result = await _sut.GetIncludingDeletedByNameAndDobAsync(
            patient.UserId,
            patient.FullName,
            patient.DateOfBirth,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(patient);
    }

    [Fact]
    public async Task GetIncludingDeletedByNameAndDobAsync_ShouldReturnPatient_WhenSoftDeleted()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();

        patient.CloseAccount(false);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetIncludingDeletedByNameAndDobAsync(
            patient.UserId,
            patient.FullName,
            patient.DateOfBirth,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(patient);
    }

    [Fact]
    public async Task GetIncludingDeletedByNameAndDobAsync_ShouldReturnNull_WhenDoesNotMatch()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();

        // Act
        var result = await _sut.GetIncludingDeletedByNameAndDobAsync(
            patient.UserId,
            PersonName.Create("Nonexistent Name"),
            patient.DateOfBirth,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
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

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<Patient> CreateSelfPatientAsync()
    {
        var user = await CreateUserAsync();

        return await CreateSelfPatientAsync(user.Id, "John Doe", new DateOnly(1990, 1, 1));
    }

    private async Task<Patient> CreateSelfPatientAsync(
        Guid userId,
        string fullName,
        DateOnly dateOfBirth,
        string bloodType = "O+"
    )
    {
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-01-01T00:00:00Z", CultureInfo.InvariantCulture)
        );

        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create(fullName),
            dateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create(bloodType), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Contact", "555-9999"));

        Context.Patients.Add(patient);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return patient;
    }

    private async Task<Patient> CreateFamilyMemberPatientAsync(
        Guid userId,
        string fullName,
        DateOnly dateOfBirth,
        string bloodType = "A+"
    )
    {
        var patient = Patient.CreateFamilyMember(
            userId,
            PersonName.Create(fullName),
            PatientRelationship.Child,
            dateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create(bloodType), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Contact", "555-9999"));

        Context.Patients.Add(patient);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return patient;
    }
}

using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
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
        var patient = Patient.CreateProfile(
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
    public async Task CreateAsync_ShouldNotReAdd_WhenEntityIsAlreadyTracked()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("New Patient"),
            new DateOnly(1990, 1, 1),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Contact", "555-9999"));

        Context.Patients.Add(patient);
        Context.Entry(patient).State = EntityState.Unchanged;

        // Act
        await _sut.CreateAsync(patient, TestContext.Current.CancellationToken);

        // Assert
        Context.Entry(patient).State.Should().Be(EntityState.Unchanged);
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
    public async Task GetByNameAndDobAsync_ShouldReturnPatient_WhenExists()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();

        // Act
        var result = await _sut.GetByNameAndDobAsync(
            patient.FullName,
            patient.DateOfBirth,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(patient);
    }

    [Fact]
    public async Task GetByNameAndDobAsync_ShouldReturnNull_WhenDoesNotMatch()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();

        // Act
        var result = await _sut.GetByNameAndDobAsync(
            PersonName.Create("Nonexistent Name"),
            patient.DateOfBirth,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    private async Task<Patient> CreateSelfPatientAsync()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("fullName"),
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

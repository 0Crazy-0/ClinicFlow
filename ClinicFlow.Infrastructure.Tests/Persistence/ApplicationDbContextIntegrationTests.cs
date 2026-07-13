using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Infrastructure.Tests.Persistence;

public class ApplicationDbContextIntegrationTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly ApplicationDbContext _sut = fixture.Context;
    private readonly FakeTimeProvider _fakeTime = new();

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
    public async Task QueryFilter_ShouldFilterOutSoftDeletedEntities_ByDefault()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync(user.Id);

        patient.CloseAccount(hasPendingAppointments: false);
        await _sut.SaveChangesAsync(TestContext.Current.CancellationToken);
        _sut.ChangeTracker.Clear();

        // Act
        var activePatients = await _sut
            .Patients.Where(p => p.Id == patient.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        activePatients.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryFilter_ShouldReturnActiveEntities_ByDefault()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync(user.Id);

        _sut.ChangeTracker.Clear();

        // Act
        var activePatients = await _sut
            .Patients.Where(p => p.Id == patient.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        activePatients.Should().ContainSingle().Which.Should().BeEquivalentTo(patient);
        activePatients[0].IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task QueryFilter_ShouldReturnSoftDeletedEntities_WhenIgnoreQueryFiltersIsUsed()
    {
        // Arrange
        var user = await CreateUserAsync();
        var patient = await CreatePatientAsync(user.Id);
        patient.CloseAccount(hasPendingAppointments: false);

        await _sut.SaveChangesAsync(TestContext.Current.CancellationToken);

        _sut.ChangeTracker.Clear();

        // Act
        var allPatients = await _sut
            .Patients.IgnoreQueryFilters()
            .Where(p => p.Id == patient.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        allPatients.Should().ContainSingle().Which.Should().BeEquivalentTo(patient);
        allPatients[0].IsDeleted.Should().BeTrue();
    }

    private async Task<User> CreateUserAsync()
    {
        var user = User.Create(
            EmailAddress.Create($"{Guid.NewGuid()}@clinic.com"),
            "password",
            PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}"),
            UserRole.Patient
        );

        _sut.Users.Add(user);
        await _sut.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<Patient> CreatePatientAsync(Guid userId)
    {
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("John Doe"),
            new DateOnly(1990, 1, 1),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Contact", "555-9999"));

        _sut.Patients.Add(patient);
        await _sut.SaveChangesAsync(TestContext.Current.CancellationToken);

        return patient;
    }
}

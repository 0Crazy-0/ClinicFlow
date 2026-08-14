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
        var doctor = await CreateDoctorAsync();
        doctor.Suspend();
        await _sut.SaveChangesAsync(TestContext.Current.CancellationToken);
        _sut.ChangeTracker.Clear();

        // Act
        var activeDoctors = await _sut
            .Doctors.Where(d => d.Id == doctor.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        activeDoctors.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryFilter_ShouldReturnActiveEntities_ByDefault()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();

        _sut.ChangeTracker.Clear();

        // Act
        var activeDoctors = await _sut
            .Doctors.Where(d => d.Id == doctor.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        activeDoctors.Should().ContainSingle().Which.Should().BeEquivalentTo(doctor);
    }

    [Fact]
    public async Task QueryFilter_ShouldReturnSoftDeletedEntities_WhenIgnoreQueryFiltersIsUsed()
    {
        // Arrange
        var doctor = await CreateDoctorAsync();
        doctor.Suspend();

        await _sut.SaveChangesAsync(TestContext.Current.CancellationToken);

        doctor.ClearDomainEvents();
        _sut.ChangeTracker.Clear();

        // Act
        var allDoctors = await _sut
            .Doctors.IgnoreQueryFilters()
            .Where(d => d.Id == doctor.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        allDoctors.Should().ContainSingle().Which.Should().BeEquivalentTo(doctor);
    }

    private async Task<Doctor> CreateDoctorAsync()
    {
        var user = User.Create(
            EmailAddress.Create($"{Guid.NewGuid()}@clinic.com"),
            "password",
            PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}"),
            UserRole.Doctor
        );

        _sut.Users.Add(user);
        await _sut.SaveChangesAsync(TestContext.Current.CancellationToken);

        var specialty = MedicalSpecialty.Create("Cardiology", "Desc", 30, 24);

        _sut.MedicalSpecialties.Add(specialty);
        await _sut.SaveChangesAsync(TestContext.Current.CancellationToken);

        var doctor = Doctor.Create(
            user.Id,
            PersonName.Create("Dr. Watson"),
            MedicalLicenseNumber.Create("LicenseNumber"),
            specialty.Id,
            "Bio",
            ConsultationRoom.Create(10, "Room 10", 1)
        );

        _sut.Doctors.Add(doctor);
        await _sut.SaveChangesAsync(TestContext.Current.CancellationToken);

        return doctor;
    }
}

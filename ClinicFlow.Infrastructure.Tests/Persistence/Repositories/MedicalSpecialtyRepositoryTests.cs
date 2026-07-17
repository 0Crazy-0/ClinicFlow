using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class MedicalSpecialtyRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly MedicalSpecialtyRepository _sut = new(fixture.Context);
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
    public async Task CreateAsync_ShouldAddSpecialtyToContext()
    {
        // Arrange
        var specialty = MedicalSpecialty.Create("Ophthalmology", "Description", 30, 24);

        // Act
        await _sut.CreateAsync(specialty, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .MedicalSpecialties.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == specialty.Id, TestContext.Current.CancellationToken);

        dbResult.Should().BeEquivalentTo(specialty);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSpecialty_WhenExistsAndActive()
    {
        // Arrange
        var specialty = await CreateSpecialty("Cardiology");

        // Act
        var result = await _sut.GetByIdAsync(specialty.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(specialty);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var specialty = await CreateSpecialty();

        specialty.Deactivate(hasActiveDoctors: false);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(specialty.Id, TestContext.Current.CancellationToken);

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
    public async Task GetByIdIncludingDeletedAsync_ShouldReturnSoftDeletedEntity()
    {
        // Arrange
        var specialty = await CreateSpecialty();

        specialty.Deactivate(hasActiveDoctors: false);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdIncludingDeletedAsync(
            specialty.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(specialty);
    }

    [Fact]
    public async Task GetByIdIncludingDeletedAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetByIdIncludingDeletedAsync(
            nonExistentId,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenActiveExistsWithName()
    {
        // Arrange
        await CreateSpecialty("Pediatrics");

        // Act
        var result = await _sut.ExistsByNameAsync(
            "Pediatrics",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenNoActiveWithName()
    {
        // Arrange
        var specialty = await CreateSpecialty("Pediatrics");

        specialty.Deactivate(hasActiveDoctors: false);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameAsync(
            "Pediatrics",
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameExcludingAsync_ShouldReturnTrue_WhenAnotherActiveWithNameExists()
    {
        // Arrange
        // Duplicate names are prevented at the handler level (see DomainErrors.MedicalSpecialty.NameAlreadyExists).
        // Two are seeded here only to test the repository's exclusion logic in isolation.
        var specialty1 = await CreateSpecialty("Dermatology");
        await CreateSpecialty("Dermatology");

        // Act
        var result = await _sut.ExistsByNameExcludingAsync(
            "Dermatology",
            specialty1.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameExcludingAsync_ShouldReturnFalse_WhenOnlySelfMatchesName()
    {
        // Arrange
        var specialty = await CreateSpecialty("Dermatology");

        // Act
        var result = await _sut.ExistsByNameExcludingAsync(
            "Dermatology",
            specialty.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameExcludingAsync_ShouldReturnFalse_WhenSoftDeleted()
    {
        // Arrange
        // Duplicate names are prevented at the handler level (see DomainErrors.MedicalSpecialty.NameAlreadyExists).
        // Two are seeded here only to test the repository's exclusion logic in isolation.
        var specialty1 = await CreateSpecialty("Dermatology");
        var specialty2 = await CreateSpecialty("Dermatology");

        specialty2.Deactivate(hasActiveDoctors: false);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameExcludingAsync(
            "Dermatology",
            specialty1.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveSpecialties()
    {
        // Arrange
        var specialty1 = await CreateSpecialty("Cardiology");
        var specialty2 = await CreateSpecialty("Pediatrics");

        specialty2.Deactivate(hasActiveDoctors: false);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetAllActiveAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().ContainSingle().Which.Should().BeEquivalentTo(specialty1);
    }

    [Fact]
    public async Task GetAllIncludingDeletedAsync_ShouldReturnAllSpecialties()
    {
        // Arrange
        var specialty1 = await CreateSpecialty("Cardiology");
        var specialty2 = await CreateSpecialty("Pediatrics");

        specialty2.Deactivate(hasActiveDoctors: false);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetAllIncludingDeletedAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo([specialty1, specialty2]);
    }

    private async Task<MedicalSpecialty> CreateSpecialty(string name = "Specialty")
    {
        var specialty = MedicalSpecialty.Create(name, "Description", 30, 24);

        Context.MedicalSpecialties.Add(specialty);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return specialty;
    }
}

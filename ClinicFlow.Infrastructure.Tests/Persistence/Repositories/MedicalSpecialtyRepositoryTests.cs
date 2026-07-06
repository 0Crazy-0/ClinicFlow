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
    public async Task GetByIdAsync_ShouldReturnSpecialty_WhenExistsAndActive()
    {
        // Arrange
        var specialty = CreateSpecialty("Cardiology");

        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(specialty.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(specialty.Id);
        result.Name.Should().Be(specialty.Name);
        result.Description.Should().Be(specialty.Description);
        result.TypicalDuration.Should().Be(specialty.TypicalDuration);
        result.CancellationPolicy.Should().Be(specialty.CancellationPolicy);
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var specialty = CreateSpecialty();

        specialty.Deactivate(hasActiveDoctors: false);

        Context.MedicalSpecialties.Add(specialty);

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
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _sut.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdIncludingDeletedAsync_ShouldReturnSoftDeletedEntity()
    {
        // Arrange
        var specialty = CreateSpecialty();

        specialty.Deactivate(hasActiveDoctors: false);

        Context.MedicalSpecialties.Add(specialty);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdIncludingDeletedAsync(
            specialty.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(specialty.Id);
        result.Name.Should().Be(specialty.Name);
        result.Description.Should().Be(specialty.Description);
        result.TypicalDuration.Should().Be(specialty.TypicalDuration);
        result.CancellationPolicy.Should().Be(specialty.CancellationPolicy);
        result.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdIncludingDeletedAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

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
        var specialty = CreateSpecialty("Pediatrics");

        Context.MedicalSpecialties.Add(specialty);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        var specialty = CreateSpecialty("Pediatrics");

        specialty.Deactivate(hasActiveDoctors: false);

        Context.MedicalSpecialties.Add(specialty);
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
        var specialty1 = CreateSpecialty("Dermatology");
        var specialty2 = CreateSpecialty("Dermatology");

        Context.MedicalSpecialties.AddRange(specialty1, specialty2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        var specialty = CreateSpecialty("Dermatology");

        Context.MedicalSpecialties.Add(specialty);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        var specialty1 = CreateSpecialty("Dermatology");
        var specialty2 = CreateSpecialty("Dermatology");

        specialty2.Deactivate(hasActiveDoctors: false);

        Context.MedicalSpecialties.AddRange(specialty1, specialty2);
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
        var specialty1 = CreateSpecialty("Cardiology");
        var specialty2 = CreateSpecialty("Pediatrics");

        specialty2.Deactivate(hasActiveDoctors: false);

        Context.MedicalSpecialties.AddRange(specialty1, specialty2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetAllActiveAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().ContainSingle();
        result[0].Id.Should().Be(specialty1.Id);
    }

    [Fact]
    public async Task GetAllIncludingDeletedAsync_ShouldReturnAllSpecialties()
    {
        // Arrange
        var specialty1 = CreateSpecialty("Cardiology");
        var specialty2 = CreateSpecialty("Pediatrics");

        specialty2.Deactivate(hasActiveDoctors: false);

        Context.MedicalSpecialties.AddRange(specialty1, specialty2);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetAllIncludingDeletedAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result.Select(s => s.Id).Should().BeEquivalentTo([specialty1.Id, specialty2.Id]);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddSpecialtyToContext()
    {
        // Arrange
        var specialty = CreateSpecialty("Ophthalmology");

        // Act
        await _sut.CreateAsync(specialty, TestContext.Current.CancellationToken);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .MedicalSpecialties.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == specialty.Id, TestContext.Current.CancellationToken);

        dbResult.Should().NotBeNull();
        dbResult.Id.Should().Be(specialty.Id);
        dbResult.Name.Should().Be(specialty.Name);
        dbResult.Description.Should().Be(specialty.Description);
        dbResult.TypicalDuration.Should().Be(specialty.TypicalDuration);
        dbResult.CancellationPolicy.Should().Be(specialty.CancellationPolicy);
        dbResult.IsDeleted.Should().BeFalse();
    }

    private static MedicalSpecialty CreateSpecialty(string name = "Specialty") =>
        MedicalSpecialty.Create(name, "Description", 30, 24);
}

using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class AppointmentTypeDefinitionRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly AppointmentTypeDefinitionRepository _sut = new(fixture.Context);
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
    public async Task CreateAsync_ShouldAddAppointmentTypeToContext()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.FirstConsultation,
            "Name",
            "Description",
            EncounterDuration.FromMinutes(20),
            AgeEligibilityPolicy.NoRestriction
        );

        // Act
        await _sut.CreateAsync(appointmentType, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .AppointmentTypes.AsNoTracking()
            .FirstOrDefaultAsync(
                a => a.Id == appointmentType.Id,
                TestContext.Current.CancellationToken
            );

        dbResult.Should().BeEquivalentTo(appointmentType);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAppointmentType_WhenExistsAndActive()
    {
        // Arrange
        var appointmentTypes = await CreateAppointmentType();

        // Act
        var result = await _sut.GetByIdAsync(
            appointmentTypes.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(appointmentTypes);
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
    public async Task GetByIdAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var appointmentTypes = await CreateAppointmentType();

        appointmentTypes.Deactivate();

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(
            appointmentTypes.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeRequiredTemplates()
    {
        // Arrange
        var appointmentTypes = await CreateAppointmentType();
        var template = ClinicalFormTemplate.Create("TEMP01", "Vitals Form", "Description", "{}");

        Context.ClinicalFormTemplates.Add(template);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        appointmentTypes.AddRequiredTemplate(template);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdAsync(
            appointmentTypes.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(appointmentTypes);
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveAppointmentTypes()
    {
        // Arrange
        var active1 = await CreateAppointmentType();
        var active2 = await CreateAppointmentType();
        var inactive = await CreateAppointmentType();

        inactive.Deactivate();

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetAllActiveAsync(TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEquivalentTo([active1, active2]);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnOnlyMatchingActiveCategory()
    {
        // Arrange
        var firstConsult = await CreateAppointmentType(
            category: AppointmentCategory.FirstConsultation
        );
        await CreateAppointmentType(category: AppointmentCategory.FollowUp);

        var inactiveFirst = await CreateAppointmentType(
            category: AppointmentCategory.FirstConsultation
        );

        inactiveFirst.Deactivate();

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetByCategoryAsync(
            AppointmentCategory.FirstConsultation,
            TestContext.Current.CancellationToken
        );

        // Assert
        results.Should().ContainSingle().Which.Should().BeEquivalentTo(firstConsult);
    }

    [Fact]
    public async Task GetEligibleByAgeAsync_ShouldReturnDefinitions_WhenAgeIsEligible()
    {
        // Arrange
        var noRestriction = await CreateAppointmentType(AgeEligibilityPolicy.NoRestriction);
        var kidOnly = await CreateAppointmentType(AgeEligibilityPolicy.Create(0, 12, false));
        await CreateAppointmentType(AgeEligibilityPolicy.Create(18, 120, false));
        var inactive = await CreateAppointmentType(AgeEligibilityPolicy.Create(0, 12, false));

        inactive.Deactivate();

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var results = await _sut.GetEligibleByAgeAsync(10, TestContext.Current.CancellationToken);

        // Assert
        results.Should().BeEquivalentTo([noRestriction, kidOnly]);
    }

    [Fact]
    public async Task GetEligibleByAgeAsync_ShouldReturnDefinitions_WhenAgeIsExactlyMinimumAge()
    {
        // Arrange
        var adultOnly = await CreateAppointmentType(AgeEligibilityPolicy.Create(18, 120, false));

        // Act
        var results = await _sut.GetEligibleByAgeAsync(18, TestContext.Current.CancellationToken);

        // Assert
        results.Should().ContainSingle().Which.Should().BeEquivalentTo(adultOnly);
    }

    [Fact]
    public async Task GetEligibleByAgeAsync_ShouldReturnDefinitions_WhenAgeIsExactlyMaximumAge()
    {
        // Arrange
        var kidOnly = await CreateAppointmentType(AgeEligibilityPolicy.Create(0, 12, false));

        // Act
        var results = await _sut.GetEligibleByAgeAsync(12, TestContext.Current.CancellationToken);

        // Assert
        results.Should().ContainSingle().Which.Should().BeEquivalentTo(kidOnly);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenActiveExistsWithName()
    {
        // Arrange
        await CreateAppointmentType("Checkup");

        // Act
        var result = await _sut.ExistsByNameAsync("Checkup", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenNoActiveWithName()
    {
        // Arrange
        var inactive = await CreateAppointmentType("Checkup");

        inactive.Deactivate();

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameAsync("Checkup", TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdIncludingDeletedAsync_ShouldReturnSoftDeletedAppointmentType()
    {
        // Arrange
        var appointmentTypes = await CreateAppointmentType();

        appointmentTypes.Deactivate();

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetByIdIncludingDeletedAsync(
            appointmentTypes.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEquivalentTo(appointmentTypes);
    }

    [Fact]
    public async Task ExistsByNameExcludingAsync_ShouldReturnTrue_WhenAnotherActiveWithNameExists()
    {
        // Arrange
        // Duplicate names are prevented at the handler level (see DomainErrors.AppointmentType.NameAlreadyExists).
        // Two are seeded here only to test the repository's exclusion logic in isolation.
        var appointmentTypes1 = await CreateAppointmentType("Name1");
        await CreateAppointmentType("Name1");

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.ExistsByNameExcludingAsync(
            "Name1",
            appointmentTypes1.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameExcludingAsync_ShouldReturnFalse_WhenOnlySelfMatchesName()
    {
        // Arrange
        var appointmentTypes = await CreateAppointmentType();

        // Act
        var result = await _sut.ExistsByNameExcludingAsync(
            "Name",
            appointmentTypes.Id,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    private async Task<AppointmentTypeDefinition> CreateAppointmentType(
        AgeEligibilityPolicy? agePolicy = null,
        AppointmentCategory category = AppointmentCategory.FirstConsultation
    )
    {
        var appointmentType = AppointmentTypeDefinition.Create(
            category,
            "Name",
            "Description",
            EncounterDuration.FromMinutes(20),
            agePolicy ?? AgeEligibilityPolicy.NoRestriction
        );

        Context.AppointmentTypes.Add(appointmentType);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return appointmentType;
    }

    private async Task<AppointmentTypeDefinition> CreateAppointmentType(
        string name,
        AppointmentCategory category = AppointmentCategory.FirstConsultation
    )
    {
        var appointmentType = AppointmentTypeDefinition.Create(
            category,
            name,
            "Description",
            EncounterDuration.FromMinutes(20),
            AgeEligibilityPolicy.NoRestriction
        );

        Context.AppointmentTypes.Add(appointmentType);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return appointmentType;
    }
}
